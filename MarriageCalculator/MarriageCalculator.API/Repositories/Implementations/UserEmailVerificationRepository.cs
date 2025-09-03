using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class UserEmailVerificationRepository : IUserEmailVerificationRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public UserEmailVerificationRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<UserEmailVerification> CreateAsync(UserEmailVerification verification)
    {
        _context.UserEmailVerifications.Add(verification);
        await _context.SaveChangesAsync();
        return verification;
    }

    public async Task<UserEmailVerification?> GetValidVerificationAsync(Guid userId, string code)
    {
        return await _context.UserEmailVerifications
            .FirstOrDefaultAsync(v => v.UserId == userId && 
                                    v.VerificationCode == code && 
                                    !v.IsUsed && 
                                    v.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<UserEmailVerification?> GetByUserIdAndCodeAsync(Guid userId, string code)
    {
        return await _context.UserEmailVerifications
            .FirstOrDefaultAsync(v => v.UserId == userId && v.VerificationCode == code);
    }

    public async Task<UserEmailVerification?> MarkAsUsedAsync(int verificationId)
    {
        var verification = await _context.UserEmailVerifications.FindAsync(verificationId);
        if (verification == null) return null;

        verification.IsUsed = true;
        verification.UsedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return verification;
    }

    public async Task<bool> DeleteExpiredAsync()
    {
        var expiredVerifications = await _context.UserEmailVerifications
            .Where(v => v.ExpiresAt <= DateTime.UtcNow || v.IsUsed)
            .ToListAsync();

        if (expiredVerifications.Count == 0) return false;

        _context.UserEmailVerifications.RemoveRange(expiredVerifications);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByUserIdAsync(Guid userId)
    {
        var userVerifications = await _context.UserEmailVerifications
            .Where(v => v.UserId == userId)
            .ToListAsync();

        if (userVerifications.Count == 0) return false;

        _context.UserEmailVerifications.RemoveRange(userVerifications);
        await _context.SaveChangesAsync();
        return true;
    }
}