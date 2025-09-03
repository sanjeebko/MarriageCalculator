using MarriageCalculator.API.Data;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public DatabaseRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetTableCountAsync()
    {
        try
        {
            // Get a rough count of main tables
            var playerCount = await _context.Players.CountAsync();
            var settingsCount = await _context.GameSettings.CountAsync();
            var gameSetsCount = await _context.MarriageGameSets.CountAsync();
            var gamesCount = await _context.MarriageGames.CountAsync();
            
            return playerCount + settingsCount + gameSetsCount + gamesCount;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<string> GetProviderNameAsync()
    {
        try
        {
            await Task.CompletedTask;
            return _context.Database.ProviderName ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}