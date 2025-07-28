using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Repositories;

public class GameSettingsRepository : IGameSettingsRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public GameSettingsRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GameSettings>> GetAllAsync()
    {
        return await _context.GameSettings.ToListAsync();
    }

    public async Task<GameSettings?> GetByIdAsync(int id)
    {
        return await _context.GameSettings.FindAsync(id);
    }

    public async Task<GameSettings> CreateAsync(GameSettings settings)
    {
        _context.GameSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<GameSettings?> UpdateAsync(int id, GameSettings settings)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;

        // Update properties
        existing.Murder = settings.Murder;
        existing.Kidnap = settings.Kidnap;
        existing.SeenPoint = settings.SeenPoint;
        existing.UnseenPoint = settings.UnseenPoint;
        existing.PointRate = settings.PointRate;
        existing.Currency = settings.Currency;
        existing.Dublee = settings.Dublee;
        existing.DubleePointLess = settings.DubleePointLess;
        existing.DubleePointBonus = settings.DubleePointBonus;
        existing.FoulPoint = settings.FoulPoint;
        existing.FoulPointBonus = settings.FoulPointBonus;
        existing.Audio = settings.Audio;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var settings = await GetByIdAsync(id);
        if (settings == null) return false;

        _context.GameSettings.Remove(settings);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.GameSettings.AnyAsync(gs => gs.Id == id);
    }
}