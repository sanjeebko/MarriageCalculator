using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Services;

public class MarriageGameServices : IMarriageGameServices
{
    private readonly MarriageCalculatorDbContext _context;

    public MarriageGameServices(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task SetupDB()
    {
        try
        {
            // Only seed default data, don't try to create database
            // Database creation should be handled by migrations
            await SeedDefaultData();
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            Console.WriteLine($"Error setting up database: {ex.Message}");
            throw;
        }
    }

    public async Task SeedDefaultData()
    {
        try
        {
            // Check if we need to seed default game settings
            if (!await _context.GameSettings.AnyAsync())
            {
                var defaultSettings = new GameSettings
                {
                    Murder = true,
                    Kidnap = false,
                    SeenPoint = 3,
                    UnseenPoint = 10,
                    PointRate = 10,
                    Currency = Currency.NPR_Rupee,
                    Dublee = true,
                    DubleePointLess = true,
                    FoulPoint = 15,
                    FoulPointBonus = FoulPointBonusType.NEXT_GAME,
                    Audio = true
                };

                _context.GameSettings.Add(defaultSettings);
                await _context.SaveChangesAsync();
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
            var info = new DatabaseInfo
            {
                PlayerCount = await _context.Players.CountAsync(),
                GameSettingsCount = await _context.GameSettings.CountAsync(),
                MarriageGameSetCount = await _context.MarriageGameSets.CountAsync(),
                MarriageGameSetPlayerCount = await _context.MarriageGameSetPlayers.CountAsync(),
                MarriageGameRoundCount = await _context.MarriageGameRounds.CountAsync(),
                MarriageGameCount = await _context.MarriageGames.CountAsync(),
                MarriageGameScoreCount = await _context.MarriageGameScores.CountAsync(),
                DatabaseCreated = await _context.Database.CanConnectAsync()
            };

            return info;
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
            // Delete all data in proper order to respect foreign key constraints
            _context.MarriageGameScores.RemoveRange(_context.MarriageGameScores);
            _context.MarriageGames.RemoveRange(_context.MarriageGames);
            _context.MarriageGameRounds.RemoveRange(_context.MarriageGameRounds);
            _context.MarriageGameSetPlayers.RemoveRange(_context.MarriageGameSetPlayers);
            _context.MarriageGameSets.RemoveRange(_context.MarriageGameSets);
            _context.GameSettings.RemoveRange(_context.GameSettings);
            _context.Players.RemoveRange(_context.Players);

            await _context.SaveChangesAsync();

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
