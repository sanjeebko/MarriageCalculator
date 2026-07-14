using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Repositories;

public class FriendInviteCodeRepository : IFriendInviteCodeRepository
{
    private readonly IMongoCollection<FriendInviteCode> _collection;

    public FriendInviteCodeRepository(MongoDbContext context)
    {
        _collection = context.FriendInviteCodes;
    }

    public async Task<FriendInviteCode?> GetActiveByOwnerAsync(string ownerUserId)
    {
        var now = DateTime.UtcNow;
        return await _collection
            .Find(c => c.OwnerUserId == ownerUserId && c.ExpiresAt > now)
            .SortByDescending(c => c.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    public async Task<FriendInviteCode?> GetByCodeAsync(string code)
    {
        return await _collection.Find(c => c.Code == code).FirstOrDefaultAsync();
    }

    public async Task<FriendInviteCode> CreateAsync(FriendInviteCode inviteCode)
    {
        await _collection.InsertOneAsync(inviteCode);
        return inviteCode;
    }
}
