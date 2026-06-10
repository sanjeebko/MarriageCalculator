using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;

namespace MarriageCalculator.API.Repositories;

public class GameSettingsRepository : IGameSettingsRepository
{
    private readonly IMongoCollection<GameSettings> _collection;

    public GameSettingsRepository(MongoDbContext context)
    {
        _collection = context.GameSettings;
    }

    public async Task<IEnumerable<GameSettings>> GetAllByUserIdAsync(string userId)
    {
        return await _collection.Find(gs => gs.UserId == userId).ToListAsync();
    }

    public async Task<GameSettings?> GetByIdAsync(string id, string userId)
    {
        return await _collection.Find(gs => gs.Id == id && gs.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<GameSettings> CreateAsync(GameSettings settings)
    {
        await _collection.InsertOneAsync(settings);
        return settings;
    }

    public async Task<GameSettings?> UpdateAsync(string id, GameSettings settings, string userId)
    {
        var update = Builders<GameSettings>.Update
            .Set(gs => gs.Murder, settings.Murder)
            .Set(gs => gs.Kidnap, settings.Kidnap)
            .Set(gs => gs.SeenPoint, settings.SeenPoint)
            .Set(gs => gs.UnseenPoint, settings.UnseenPoint)
            .Set(gs => gs.PointRate, settings.PointRate)
            .Set(gs => gs.Currency, settings.Currency)
            .Set(gs => gs.Dublee, settings.Dublee)
            .Set(gs => gs.DubleePointLess, settings.DubleePointLess)
            .Set(gs => gs.DubleePointBonus, settings.DubleePointBonus)
            .Set(gs => gs.FoulPoint, settings.FoulPoint)
            .Set(gs => gs.FoulPointBonus, settings.FoulPointBonus)
            .Set(gs => gs.Audio, settings.Audio);

        return await _collection.FindOneAndUpdateAsync(
            gs => gs.Id == id && gs.UserId == userId,
            update,
            new FindOneAndUpdateOptions<GameSettings> { ReturnDocument = ReturnDocument.After });
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var result = await _collection.DeleteOneAsync(gs => gs.Id == id && gs.UserId == userId);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id, string userId)
    {
        return await _collection.CountDocumentsAsync(gs => gs.Id == id && gs.UserId == userId) > 0;
    }
}
