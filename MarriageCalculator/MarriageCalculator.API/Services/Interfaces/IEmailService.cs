namespace MarriageCalculator.API.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string email, string displayName, string verificationCode);
    Task<bool> SendPasswordResetEmailAsync(string email, string displayName, string resetToken);
}