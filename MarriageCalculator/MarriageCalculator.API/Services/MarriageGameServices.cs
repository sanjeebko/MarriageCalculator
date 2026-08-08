using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;

namespace MarriageCalculator.API.Services;

public class MarriageGameServices : IMarriageGameServices
{
    private readonly MongoDbContext _context;

    public MarriageGameServices(MongoDbContext context)
    {
        _context = context;
    }

    public async Task SetupDB()
    {
        try
        {
            await SeedDefaultData();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up database: {ex.Message}");
            throw;
        }
    }

    public async Task SeedDefaultData()
    {
        try
        {
            var count = await _context.GameSettings.CountDocumentsAsync(_ => true);
            if (count == 0)
            {
                var defaultSettings = GameSettings.Default();
                await _context.GameSettings.InsertOneAsync(defaultSettings);
                Console.WriteLine("Default game settings seeded successfully.");
            }
            else
            {
                Console.WriteLine("Game settings already exist, skipping seeding.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding default data: {ex.Message}");
            throw;
        }
    }

    public async Task<DatabaseInfo> GetDatabaseInfoAsync()
    {
        try
        {
            return new DatabaseInfo
            {
                PlayerCount = (int)await _context.Players.CountDocumentsAsync(_ => true),
                GameSettingsCount = (int)await _context.GameSettings.CountDocumentsAsync(_ => true),
                MarriageGameSetCount = (int)await _context.MarriageGameSets.CountDocumentsAsync(_ => true),
                MarriageGameSetPlayerCount = (int)await _context.MarriageGameSetPlayers.CountDocumentsAsync(_ => true),
                MarriageGameRoundCount = (int)await _context.MarriageGameRounds.CountDocumentsAsync(_ => true),
                MarriageGameCount = (int)await _context.MarriageGames.CountDocumentsAsync(_ => true),
                MarriageGameScoreCount = (int)await _context.MarriageGameScores.CountDocumentsAsync(_ => true),
                DatabaseCreated = await _context.CanConnectAsync()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting database info: {ex.Message}");
            return new DatabaseInfo { DatabaseCreated = false };
        }
    }

    public async Task CleanupDatabaseAsync()
    {
        try
        {
            // Delete all documents from all collections
            await _context.MarriageGameScores.DeleteManyAsync(_ => true);
            await _context.MarriageGames.DeleteManyAsync(_ => true);
            await _context.MarriageGameRounds.DeleteManyAsync(_ => true);
            await _context.MarriageGameSetPlayers.DeleteManyAsync(_ => true);
            await _context.MarriageGameSets.DeleteManyAsync(_ => true);
            await _context.GameSettings.DeleteManyAsync(_ => true);
            await _context.Players.DeleteManyAsync(_ => true);

            // Re-seed default data
            await SeedDefaultData();

            Console.WriteLine("Database cleanup completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up database: {ex.Message}");
            throw;
        }
    }
}

