using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;

namespace MarriageCalculator.API.Repositories;

public class EmailVerificationCodeRepository : IEmailVerificationCodeRepository
{
    private readonly MongoDbContext _context;

    public EmailVerificationCodeRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateCodeAsync(EmailVerificationCode code)
    {
        code.Email = code.Email.Trim().ToLowerInvariant();
        await _context.EmailVerificationCodes.InsertOneAsync(code);
    }

    public async Task<EmailVerificationCode?> GetLatestValidCodeAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        return await _context.EmailVerificationCodes
            .Find(c => c.Email == normalizedEmail && !c.IsUsed && c.ExpiresAt > now)
            .SortByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task MarkCodeAsUsedAsync(string id)
    {
        var update = Builders<EmailVerificationCode>.Update.Set(c => c.IsUsed, true);
        await _context.EmailVerificationCodes.UpdateOneAsync(c => c.Id == id, update);
    }
}
