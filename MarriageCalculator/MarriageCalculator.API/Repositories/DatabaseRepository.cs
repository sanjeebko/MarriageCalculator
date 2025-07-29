using MarriageCalculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Repositories;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public DatabaseRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanConnectAsync()
    {
        return await _context.Database.CanConnectAsync();
    }

    public async Task<int> GetTableCountAsync()
    {
        try
        {
            // Count existing tables by checking each DbSet
            var tableCount = 0;
            
            // Check if Players table exists
            try { await _context.Players.AnyAsync(); tableCount++; } catch { }
            
            // Check if GameSettings table exists
            try { await _context.GameSettings.AnyAsync(); tableCount++; } catch { }
            
            // Check if MarriageGameSets table exists
            try { await _context.MarriageGameSets.AnyAsync(); tableCount++; } catch { }
            
            // Check if MarriageGameSetPlayers table exists
            try { await _context.MarriageGameSetPlayers.AnyAsync(); tableCount++; } catch { }
            
            // Check if MarriageGameRounds table exists
            try { await _context.MarriageGameRounds.AnyAsync(); tableCount++; } catch { }
            
            // Check if MarriageGames table exists
            try { await _context.MarriageGames.AnyAsync(); tableCount++; } catch { }
            
            // Check if MarriageGameScores table exists
            try { await _context.MarriageGameScores.AnyAsync(); tableCount++; } catch { }
            
            return tableCount;
        }
        catch (Exception ex)
        {
            // Log error and return 0
            Console.WriteLine($"Error getting table count: {ex.Message}");
            return 0;
        }
    }

    public async Task<string> GetProviderNameAsync()
    {
        return await Task.FromResult(_context.Database.ProviderName ?? "Unknown");
    }
}