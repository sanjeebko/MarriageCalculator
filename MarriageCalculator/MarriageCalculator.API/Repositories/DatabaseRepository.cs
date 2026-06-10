using MarriageCalculator.API.Data;
using MongoDB.Driver;

namespace MarriageCalculator.API.Repositories;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly MongoDbContext _context;

    public DatabaseRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanConnectAsync()
    {
        return await _context.CanConnectAsync();
    }

    public async Task<int> GetTableCountAsync()
    {
        try
        {
            var cursor = await _context.GetDatabase().ListCollectionNamesAsync();
            var collectionNames = await cursor.ToListAsync();
            return collectionNames.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting collection count: {ex.Message}");
            return 0;
        }
    }

    public async Task<string> GetProviderNameAsync()
    {
        return await Task.FromResult("MongoDB");
    }
}
