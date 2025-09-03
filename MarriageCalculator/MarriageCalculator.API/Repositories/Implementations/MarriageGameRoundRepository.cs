using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class MarriageGameRoundRepository : IMarriageGameRoundRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public MarriageGameRoundRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MarriageGameRound>> GetAllAsync()
    {
        return await _context.MarriageGameRounds.OrderBy(r => r.Sequence).ToListAsync();
    }

    public async Task<MarriageGameRound?> GetByIdAsync(int id)
    {
        return await _context.MarriageGameRounds.FindAsync(id);
    }

    public async Task<MarriageGameRound> CreateAsync(MarriageGameRound round)
    {
        _context.MarriageGameRounds.Add(round);
        await _context.SaveChangesAsync();
        return round;
    }

    public async Task<MarriageGameRound?> UpdateAsync(int id, MarriageGameRound round)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;

        existing.Sequence = round.Sequence;
        existing.MarriageGameSetId = round.MarriageGameSetId;
        existing.Completed = round.Completed;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var round = await GetByIdAsync(id);
        if (round == null) return false;

        _context.MarriageGameRounds.Remove(round);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.MarriageGameRounds.AnyAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<MarriageGameRound>> GetByGameSetIdAsync(int gameSetId)
    {
        return await _context.MarriageGameRounds
            .Where(r => r.MarriageGameSetId == gameSetId)
            .OrderBy(r => r.Sequence)
            .ToListAsync();
    }
}