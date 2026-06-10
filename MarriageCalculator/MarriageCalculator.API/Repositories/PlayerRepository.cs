using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;

namespace MarriageCalculator.API.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly IMongoCollection<Player> _collection;

    public PlayerRepository(MongoDbContext context)
    {
        _collection = context.Players;
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        return await _collection.Find(p => !p.Deleted).ToListAsync();
    }

    public async Task<Player?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id && !p.Deleted).FirstOrDefaultAsync();
    }

    public async Task<Player> CreateAsync(Player player)
    {
        await _collection.InsertOneAsync(player);
        return player;
    }

    public async Task<Player?> UpdateAsync(string id, Player player)
    {
        var update = Builders<Player>.Update
            .Set(p => p.Name, player.Name)
            .Set(p => p.Email, player.Email);

        var result = await _collection.FindOneAndUpdateAsync(
            p => p.Id == id && !p.Deleted,
            update,
            new FindOneAndUpdateOptions<Player> { ReturnDocument = ReturnDocument.After });

        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<Player>.Update.Set(p => p.Deleted, true);
        var result = await _collection.UpdateOneAsync(p => p.Id == id && !p.Deleted, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _collection.CountDocumentsAsync(p => p.Id == id && !p.Deleted) > 0;
    }
}
