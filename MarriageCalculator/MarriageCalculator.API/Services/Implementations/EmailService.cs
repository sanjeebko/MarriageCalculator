using System.Net;
using System.Net.Mail;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendVerificationEmailAsync(string email, string displayName, string verificationCode)
    {
        try
        {
            var emailBody = $@"
<html>
<body>
    <h2>Email Verification - Marriage Calculator</h2>
    <p>Hello <strong>{displayName}</strong>,</p>
    
    <p>Thank you for registering with Marriage Calculator!</p>
    
    <p>Your email verification code is: <strong style='font-size: 18px; color: #007bff;'>{verificationCode}</strong></p>
    
    <p>This code will expire in 2 hours. Please use this code to verify your email address and complete your registration.</p>
    
    <p>If you didn't request this verification, please ignore this email.</p>
    
    <hr style='margin: 20px 0;'>
    <p style='font-size: 12px; color: #666;'>
        <strong>This is an automated email from Marriage Calculator.</strong><br>
        If you received this email unexpectedly, please delete this message immediately.
    </p>
    
    <p>Best regards,<br>
    <strong>Marriage Calculator Team</strong></p>
</body>
</html>";

            return await SendEmailAsync(email, "Verify Your Email Address", emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string displayName, string resetToken)
    {
        try
        {
            var emailBody = $@"
<html>
<body>
    <h2>Password Reset Request - Marriage Calculator</h2>
    <p>Hello <strong>{displayName}</strong>,</p>
    
    <p>You requested a password reset for your Marriage Calculator account.</p>
    
    <p>Your password reset token is: <strong style='font-size: 18px; color: #dc3545;'>{resetToken}</strong></p>
    
    <p>This token will expire in 1 hour. Please use this token to reset your password.</p>
    
    <p>If you didn't request a password reset, please ignore this email.</p>
    
    <hr style='margin: 20px 0;'>
    <p style='font-size: 12px; color: #666;'>
        <strong>This is an automated email from Marriage Calculator.</strong><br>
        If you received this email unexpectedly, please delete this message immediately.
    </p>
    
    <p>Best regards,<br>
    <strong>Marriage Calculator Team</strong></p>
</body>
</html>";

            return await SendEmailAsync(email, "Password Reset Request", emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Get SMTP configuration from environment variables
            var smtpServer = Environment.GetEnvironmentVariable("MCSMTP");
            var fromMail = Environment.GetEnvironmentVariable("MCMAILUSERNAME");
            var fromPassword = Environment.GetEnvironmentVariable("MCMAILPASSWORD");

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(fromMail) || string.IsNullOrEmpty(fromPassword))
            {
                _logger.LogError("SMTP configuration missing. Required environment variables: MCSMTP, MCMAILUSERNAME, MCMAILPASSWORD");
                return false;
            }

            _logger.LogInformation("Sending email - To: {Email}, Subject: {Subject}, SMTP: {SmtpServer}", toEmail, subject, smtpServer);

            // Determine optimal port and SSL settings based on SMTP server
            var (port, enableSsl) = GetOptimalSmtpSettings(smtpServer);
            _logger.LogDebug("Using SMTP settings - Port: {Port}, SSL: {EnableSsl}", port, enableSsl);

            using var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = enableSsl,
                Timeout = 30000, // 30 seconds timeout
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromMail, "Marriage Calculator"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            mailMessage.To.Add(toEmail);

            // Add headers for better deliverability
            mailMessage.Headers.Add("X-Mailer", "Marriage Calculator API");
            mailMessage.Headers.Add("X-Priority", "3"); // Normal priority
            
            stopwatch.Stop();
            var setupTime = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Restart();
            await smtpClient.SendMailAsync(mailMessage);
            stopwatch.Stop();
            
            _logger.LogInformation("Email sent successfully to {Email} in {TotalTime}ms (Setup: {SetupTime}ms, Send: {SendTime}ms)", 
                toEmail, setupTime + stopwatch.ElapsedMilliseconds, setupTime, stopwatch.ElapsedMilliseconds);
            return true;
        }
        catch (SmtpException smtpEx)
        {
            stopwatch.Stop();
            _logger.LogError(smtpEx, "SMTP error sending email to {Email} after {ElapsedMs}ms. Status: {StatusCode}", 
                toEmail, stopwatch.ElapsedMilliseconds, smtpEx.StatusCode);
            
            // Log specific SMTP error guidance
            LogSmtpErrorGuidance(smtpEx);
            return false;
        }
        catch (System.Net.Sockets.SocketException socketEx)
        {
            stopwatch.Stop();
            _logger.LogError(socketEx, "Network connectivity error sending email to {Email} after {ElapsedMs}ms. Error Code: {ErrorCode}", 
                toEmail, stopwatch.ElapsedMilliseconds, socketEx.ErrorCode);
            
            LogNetworkErrorGuidance(socketEx);
            return false;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to send email to {Email} after {ElapsedMs}ms", toEmail, stopwatch.ElapsedMilliseconds);
            return false;
        }
    }

    /// <summary>
    /// Get optimal SMTP settings based on the server
    /// </summary>
    private static (int port, bool enableSsl) GetOptimalSmtpSettings(string smtpServer)
    {
        return smtpServer.ToLowerInvariant() switch
        {
            // Zoho settings
            "smtp.zoho.com" or "smtp.zoho.eu" or "smtp.zoho.in" or "smtp.zoho.com.au" => (587, true),
            
            // Gmail settings
            "smtp.gmail.com" => (587, true),
            
            // Outlook/Hotmail settings
            "smtp-mail.outlook.com" or "smtp.live.com" => (587, true),
            
            // Yahoo settings
            "smtp.mail.yahoo.com" => (587, true),
            
            // SendGrid
            "smtp.sendgrid.net" => (587, true),
            
            // Amazon SES
            var server when server.Contains("amazonses.com") => (587, true),
            
            // Default to standard SMTP with TLS
            _ => (587, true)
        };
    }

    /// <summary>
    /// Log specific guidance for SMTP errors
    /// </summary>
    private void LogSmtpErrorGuidance(SmtpException smtpEx)
    {
        switch (smtpEx.StatusCode)
        {
            case SmtpStatusCode.GeneralFailure:
            case SmtpStatusCode.CommandUnrecognized:
                _logger.LogWarning("SMTP Authentication or command error. For Zoho: 1) Enable IMAP/POP3 in account settings, 2) Use App Password if 2FA enabled, 3) Check username format (full email vs username only)");
                break;
            
            case SmtpStatusCode.SyntaxError:
            case SmtpStatusCode.CommandNotImplemented:
                _logger.LogWarning("SMTP command error. Check SMTP server configuration and credentials.");
                break;
            
            case SmtpStatusCode.MailboxBusy:
            case SmtpStatusCode.TransactionFailed:
                _logger.LogWarning("SMTP server temporarily unavailable. This may be due to rate limiting or server maintenance. Consider implementing retry logic.");
                break;
            
            case SmtpStatusCode.InsufficientStorage:
                _logger.LogWarning("Recipient mailbox full. The email cannot be delivered because the recipient's mailbox has exceeded its storage limit.");
                break;
            
            case SmtpStatusCode.ClientNotPermitted:
                _logger.LogWarning("Client not permitted. For Zoho: Check if your IP is whitelisted and account has proper sending permissions.");
                break;
            
            case SmtpStatusCode.MailboxUnavailable:
                _logger.LogWarning("Mailbox unavailable. Check if the recipient email address is valid and exists.");
                break;
            
            default:
                _logger.LogWarning("SMTP Error Code: {StatusCode}. Check SMTP server documentation for specific error details.", smtpEx.StatusCode);
                break;
        }
    }

    /// <summary>
    /// Log specific guidance for network errors
    /// </summary>
    private void LogNetworkErrorGuidance(System.Net.Sockets.SocketException socketEx)
    {
        switch (socketEx.ErrorCode)
        {
            case 11: // Resource temporarily unavailable
                _logger.LogWarning("Network resource temporarily unavailable. This may indicate: 1) Firewall blocking SMTP ports, 2) ISP blocking email traffic, 3) Server overload. Try different network or VPN.");
                break;
            
            case 10060: // Connection timed out
                _logger.LogWarning("Connection timed out. This may indicate: 1) Firewall blocking port 587, 2) Incorrect SMTP server address, 3) Network connectivity issues.");
                break;
            
            case 10061: // Connection refused
                _logger.LogWarning("Connection refused. This may indicate: 1) SMTP server is down, 2) Wrong port number, 3) Firewall blocking connection.");
                break;
            
            case 11001: // Host not found
                _logger.LogWarning("SMTP server hostname could not be resolved. Check the SMTP server address: {SmtpServer}", Environment.GetEnvironmentVariable("MCSMTP"));
                break;
            
            default:
                _logger.LogWarning("Network error code: {ErrorCode}. Check network connectivity and firewall settings.", socketEx.ErrorCode);
                break;
        }
    }
}