using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

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
            // Only create a system user if no users exist at all (initial database setup)
            // Don't create any GameSettings here - those should be user-specific and created during registration
            
            if (!await _context.Users.AnyAsync())
            {
                var systemUser = new User
                {
                    DisplayName = "System",
                    Email = "system@marriagecalculator.com",
                    PasswordHash = "temp_hash",
                    Salt = "temp_salt",
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(systemUser);
                await _context.SaveChangesAsync();
                Console.WriteLine("Default system user created successfully.");
                
                // Create GameSettings only for the system user (this won't affect regular users)
                var systemGameSettings = new GameSettings
                {
                    UserId = systemUser.Id,
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
                    Audio = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.GameSettings.Add(systemGameSettings);
                await _context.SaveChangesAsync();
                Console.WriteLine("Default system GameSettings created successfully.");
            }
            else
            {
                Console.WriteLine("Users already exist, skipping system data seeding.");
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