using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for UserEmailVerification operations
/// </summary>
public interface IUserEmailVerificationRepository
{
    Task<UserEmailVerification> CreateAsync(UserEmailVerification verification);
    Task<UserEmailVerification?> GetValidVerificationAsync(Guid userId, string code);
    Task<UserEmailVerification?> GetByUserIdAndCodeAsync(Guid userId, string code);
    Task<UserEmailVerification?> MarkAsUsedAsync(int verificationId);
    Task<bool> DeleteExpiredAsync();
    Task<bool> DeleteByUserIdAsync(Guid userId);
}