using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories;

public interface IEmailVerificationCodeRepository
{
    Task CreateCodeAsync(EmailVerificationCode code);
    Task<EmailVerificationCode?> GetLatestValidCodeAsync(string email);
    Task MarkCodeAsUsedAsync(string id);
}
