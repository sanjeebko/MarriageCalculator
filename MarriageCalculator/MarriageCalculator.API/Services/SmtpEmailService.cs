using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

/// <summary>
/// SMTP email sender (System.Net.Mail — no extra packages). Works with any SMTP
/// provider (Brevo, SendGrid SMTP relay, Mailgun, Gmail app password).
/// Configure via the "Email" section (see appsettings.json); when Email:Host is
/// empty the service is a logged no-op so development works without a provider.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_configuration["Email:Host"]);

    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Email not configured (Email:Host is empty) — skipped sending \"{Subject}\" to {To}.", subject, toEmail);
            return false;
        }

        try
        {
            var host = _configuration["Email:Host"]!;
            var port = int.TryParse(_configuration["Email:Port"], out var p) ? p : 587;
            var user = _configuration["Email:User"];
            var password = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"] ?? user ?? "noreply@localhost";
            var fromName = _configuration["Email:FromName"] ?? "Marriage Calculator";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = string.IsNullOrEmpty(user)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(user, password),
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email \"{Subject}\" to {To}.", subject, toEmail);
            return false;
        }
    }
}
