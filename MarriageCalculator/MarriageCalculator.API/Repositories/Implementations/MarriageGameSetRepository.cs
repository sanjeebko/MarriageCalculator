using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class MarriageGameSetRepository : IMarriageGameSetRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public MarriageGameSetRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MarriageGameSet>> GetAllAsync()
    {
        return await _context.MarriageGameSets.OrderByDescending(gs => gs.Created).ToListAsync();
    }

    public async Task<IEnumerable<MarriageGameSet>> GetByGameSettingsIdAsync(int gameSettingsId)
    {
        return await _context.MarriageGameSets
            .Where(gs => gs.GameSettingsId == gameSettingsId)
            .OrderByDescending(gs => gs.Created)
            .ToListAsync();
    }

    public async Task<MarriageGameSet?> GetByIdAsync(int id)
    {
        return await _context.MarriageGameSets.FindAsync(id);
    }

    public async Task<MarriageGameSet> CreateAsync(MarriageGameSet gameSet)
    {
        _context.MarriageGameSets.Add(gameSet);
        await _context.SaveChangesAsync();
        return gameSet;
    }

    public async Task<MarriageGameSet?> UpdateAsync(int id, MarriageGameSet gameSet)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;

        existing.Name = gameSet.Name;
        existing.LastPlayed = gameSet.LastPlayed;
        existing.IsActive = gameSet.IsActive;
        existing.GameSettingsId = gameSet.GameSettingsId;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var gameSet = await GetByIdAsync(id);
        if (gameSet == null) return false;

        _context.MarriageGameSets.Remove(gameSet);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.MarriageGameSets.AnyAsync(gs => gs.Id == id);
    }

    public async Task<MarriageGameSet?> GetLatestActiveAsync()
    {
        return await _context.MarriageGameSets
            .Where(gs => gs.IsActive)
            .OrderByDescending(gs => gs.LastPlayed)
            .FirstOrDefaultAsync();
    }

    public async Task<MarriageGameSet?> GetLatestActiveForUserAsync(Guid userId)
    {
        return await _context.MarriageGameSets
            .Join(_context.GameSettings,
                gs => gs.GameSettingsId,
                settings => settings.Id,
                (gs, settings) => new { GameSet = gs, Settings = settings })
            .Where(joined => joined.Settings.UserId == userId && joined.GameSet.IsActive)
            .OrderByDescending(joined => joined.GameSet.LastPlayed)
            .Select(joined => joined.GameSet)
            .FirstOrDefaultAsync();
    }

    public async Task<MarriageGameSet?> GetActiveByGameSettingsIdAsync(int gameSettingsId)
    {
        return await _context.MarriageGameSets
            .Where(gs => gs.GameSettingsId == gameSettingsId && gs.IsActive)
            .FirstOrDefaultAsync();
    }
}