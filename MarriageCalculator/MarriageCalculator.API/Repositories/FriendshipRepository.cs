using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly IMongoCollection<Friendship> _collection;

    public FriendshipRepository(MongoDbContext context)
    {
        _collection = context.Friendships;
    }

    public async Task<IEnumerable<Friendship>> GetAllForUserAsync(string userId)
    {
        return await _collection.Find(f => f.RequesterUserId == userId || f.ReceiverUserId == userId).ToListAsync();
    }

    public async Task<Friendship?> GetByIdAsync(string id)
    {
        return await _collection.Find(f => f.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Friendship?> GetByUsersAsync(string requesterId, string receiverId)
    {
        return await _collection.Find(f => 
            (f.RequesterUserId == requesterId && f.ReceiverUserId == receiverId) || 
            (f.RequesterUserId == receiverId && f.ReceiverUserId == requesterId)
        ).FirstOrDefaultAsync();
    }

    public async Task<Friendship> CreateAsync(Friendship friendship)
    {
        await _collection.InsertOneAsync(friendship);
        return friendship;
    }

    public async Task<Friendship?> UpdateAsync(string id, Friendship friendship)
    {
        var update = Builders<Friendship>.Update
            .Set(f => f.Status, friendship.Status)
            .Set(f => f.ActionAt, friendship.ActionAt);

        var result = await _collection.FindOneAndUpdateAsync(
            f => f.Id == id,
            update,
            new FindOneAndUpdateOptions<Friendship> { ReturnDocument = ReturnDocument.After });

        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(f => f.Id == id);
        return result.DeletedCount > 0;
    }
}
