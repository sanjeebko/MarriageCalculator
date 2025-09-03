using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class MarriageGameRepository : IMarriageGameRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public MarriageGameRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MarriageGame>> GetAllAsync()
    {
        return await _context.MarriageGames.OrderByDescending(g => g.CreatedTime).ToListAsync();
    }

    public async Task<MarriageGame?> GetByIdAsync(int id)
    {
        return await _context.MarriageGames.FindAsync(id);
    }

    public async Task<MarriageGame> CreateAsync(MarriageGame game)
    {
        _context.MarriageGames.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task<MarriageGame?> UpdateAsync(int id, MarriageGame game)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;

        existing.Sequence = game.Sequence;
        existing.MarriageGameRoundId = game.MarriageGameRoundId;
        existing.WinnerId = game.WinnerId;
        existing.DealerId = game.DealerId;
        existing.TotalMaal = game.TotalMaal;
        existing.ClosedRound = game.ClosedRound;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteGameAsync(int id)
    {
        var game = await GetByIdAsync(id);
        if (game == null) return false;

        _context.MarriageGames.Remove(game);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.MarriageGames.AnyAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<MarriageGame>> GetByRoundIdAsync(int roundId)
    {
        return await _context.MarriageGames
            .Where(g => g.MarriageGameRoundId == roundId)
            .OrderBy(g => g.Sequence)
            .ToListAsync();
    }
}