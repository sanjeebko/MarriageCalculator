using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for RefreshToken operations
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId);
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdListAsync(Guid userId);
    Task<RefreshToken?> UpdateAsync(RefreshToken refreshToken);
    Task<bool> RevokeAsync(string token, string reason);
    Task<bool> RevokeAllByUserIdAsync(Guid userId, string reason);
    Task<bool> DeleteExpiredAsync();
    Task<bool> DeleteByUserIdAsync(Guid userId);
}