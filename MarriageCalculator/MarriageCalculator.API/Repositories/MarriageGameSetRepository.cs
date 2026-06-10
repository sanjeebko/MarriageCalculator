using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;

namespace MarriageCalculator.API.Repositories;

public class MarriageGameSetRepository : IMarriageGameSetRepository
{
    private readonly IMongoCollection<MarriageGameSet> _collection;

    public MarriageGameSetRepository(MongoDbContext context)
    {
        _collection = context.MarriageGameSets;
    }

    public async Task<IEnumerable<MarriageGameSet>> GetAllByHostUserIdAsync(string hostUserId)
    {
        return await _collection.Find(gs => gs.HostUserId == hostUserId)
            .SortByDescending(gs => gs.Created)
            .ToListAsync();
    }

    public async Task<MarriageGameSet?> GetByIdAsync(string id, string hostUserId)
    {
        return await _collection.Find(gs => gs.Id == id && gs.HostUserId == hostUserId).FirstOrDefaultAsync();
    }

    public async Task<MarriageGameSet> CreateAsync(MarriageGameSet gameSet)
    {
        await _collection.InsertOneAsync(gameSet);
        return gameSet;
    }

    public async Task<MarriageGameSet?> UpdateAsync(string id, MarriageGameSet gameSet, string hostUserId)
    {
        var update = Builders<MarriageGameSet>.Update
            .Set(gs => gs.Name, gameSet.Name)
            .Set(gs => gs.LastPlayed, gameSet.LastPlayed)
            .Set(gs => gs.IsActive, gameSet.IsActive)
            .Set(gs => gs.GameSettingsId, gameSet.GameSettingsId);

        return await _collection.FindOneAndUpdateAsync(
            gs => gs.Id == id && gs.HostUserId == hostUserId,
            update,
            new FindOneAndUpdateOptions<MarriageGameSet> { ReturnDocument = ReturnDocument.After });
    }

    public async Task<bool> DeleteAsync(string id, string hostUserId)
    {
        var result = await _collection.DeleteOneAsync(gs => gs.Id == id && gs.HostUserId == hostUserId);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id, string hostUserId)
    {
        return await _collection.CountDocumentsAsync(gs => gs.Id == id && gs.HostUserId == hostUserId) > 0;
    }

    public async Task<MarriageGameSet?> GetLatestActiveAsync(string hostUserId)
    {
        return await _collection.Find(gs => gs.IsActive && gs.HostUserId == hostUserId)
            .SortByDescending(gs => gs.LastPlayed)
            .FirstOrDefaultAsync();
    }
}
