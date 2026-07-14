using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Repositories;

public class PendingEmailInviteRepository : IPendingEmailInviteRepository
{
    private readonly IMongoCollection<PendingEmailInvite> _collection;

    public PendingEmailInviteRepository(MongoDbContext context)
    {
        _collection = context.PendingEmailInvites;
    }

    public async Task<IEnumerable<PendingEmailInvite>> GetPendingByEmailAsync(string email)
    {
        var now = DateTime.UtcNow;
        return await _collection
            .Find(i => i.InviteeEmail == email && i.Status == "Pending" && i.ExpiresAt > now)
            .ToListAsync();
    }

    public async Task<PendingEmailInvite?> GetPendingByInviterAndEmailAsync(string inviterUserId, string email)
    {
        var now = DateTime.UtcNow;
        return await _collection
            .Find(i => i.InviterUserId == inviterUserId && i.InviteeEmail == email
                       && i.Status == "Pending" && i.ExpiresAt > now)
            .FirstOrDefaultAsync();
    }

    public async Task<PendingEmailInvite> CreateAsync(PendingEmailInvite invite)
    {
        await _collection.InsertOneAsync(invite);
        return invite;
    }

    public async Task<bool> MarkClaimedAsync(string id)
    {
        var update = Builders<PendingEmailInvite>.Update.Set(i => i.Status, "Claimed");
        var result = await _collection.UpdateOneAsync(i => i.Id == id, update);
        return result.ModifiedCount > 0;
    }
}
