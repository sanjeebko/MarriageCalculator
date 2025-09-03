using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public RefreshTokenRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && 
                        rt.IsActive && 
                        !rt.IsRevoked && 
                        rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdListAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && 
                        rt.IsActive && 
                        !rt.IsRevoked && 
                        rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<RefreshToken?> UpdateAsync(RefreshToken refreshToken)
    {
        var existing = await GetByTokenAsync(refreshToken.Token);
        if (existing == null) return null;

        existing.IsActive = refreshToken.IsActive;
        existing.RevokedAt = refreshToken.RevokedAt;
        existing.RevokedReason = refreshToken.RevokedReason;
        existing.ReplacedByToken = refreshToken.ReplacedByToken;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> RevokeAsync(string token, string reason)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken == null) return false;

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedReason = reason;
        refreshToken.IsActive = false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAllByUserIdAsync(Guid userId, string reason)
    {
        var activeTokens = await GetActiveByUserIdListAsync(userId);
        if (!activeTokens.Any()) return false;

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
            token.IsActive = false;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteExpiredAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.IsRevoked)
            .ToListAsync();

        if (expiredTokens.Count == 0) return false;

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByUserIdAsync(Guid userId)
    {
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();

        if (userTokens.Count == 0) return false;

        _context.RefreshTokens.RemoveRange(userTokens);
        await _context.SaveChangesAsync();
        return true;
    }
}