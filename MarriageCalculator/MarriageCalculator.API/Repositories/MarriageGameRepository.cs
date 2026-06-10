using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;

namespace MarriageCalculator.API.Repositories;

public class MarriageGameRepository : IMarriageGameRepository
{
    private readonly IMongoCollection<MarriageGame> _collection;

    public MarriageGameRepository(MongoDbContext context)
    {
        _collection = context.MarriageGames;
    }

    public async Task<IEnumerable<MarriageGame>> GetAllAsync()
    {
        return await _collection.Find(_ => true)
            .SortByDescending(g => g.CreatedTime)
            .ToListAsync();
    }

    public async Task<MarriageGame?> GetByIdAsync(string id)
    {
        return await _collection.Find(g => g.Id == id).FirstOrDefaultAsync();
    }

    public async Task<MarriageGame> CreateAsync(MarriageGame game)
    {
        await _collection.InsertOneAsync(game);
        return game;
    }

    public async Task<MarriageGame?> UpdateAsync(string id, MarriageGame game)
    {
        var update = Builders<MarriageGame>.Update
            .Set(g => g.Sequence, game.Sequence)
            .Set(g => g.MarriageGameRoundId, game.MarriageGameRoundId)
            .Set(g => g.WinnerId, game.WinnerId)
            .Set(g => g.DealerId, game.DealerId)
            .Set(g => g.TotalMaal, game.TotalMaal)
            .Set(g => g.ClosedRound, game.ClosedRound);

        return await _collection.FindOneAndUpdateAsync(
            g => g.Id == id,
            update,
            new FindOneAndUpdateOptions<MarriageGame> { ReturnDocument = ReturnDocument.After });
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(g => g.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _collection.CountDocumentsAsync(g => g.Id == id) > 0;
    }

    public async Task<IEnumerable<MarriageGame>> GetByRoundIdAsync(string roundId)
    {
        return await _collection.Find(g => g.MarriageGameRoundId == roundId)
            .SortBy(g => g.Sequence)
            .ToListAsync();
    }
}
