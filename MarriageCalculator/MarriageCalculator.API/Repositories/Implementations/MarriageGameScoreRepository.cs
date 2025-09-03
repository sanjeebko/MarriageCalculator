using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class MarriageGameScoreRepository : IMarriageGameScoreRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public MarriageGameScoreRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MarriageGameScore>> GetAllAsync()
    {
        return await _context.MarriageGameScores
            .OrderBy(s => s.MarriageGameId)
            .ThenBy(s => s.Position)
            .ToListAsync();
    }

    public async Task<MarriageGameScore?> GetByIdAsync(int id)
    {
        return await _context.MarriageGameScores.FindAsync(id);
    }

    public async Task<MarriageGameScore> CreateAsync(MarriageGameScore score)
    {
        _context.MarriageGameScores.Add(score);
        await _context.SaveChangesAsync();
        return score;
    }

    public async Task<MarriageGameScore?> UpdateAsync(int id, MarriageGameScore score)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) return null;

        existing.MarriageGameId = score.MarriageGameId;
        existing.PlayerId = score.PlayerId;
        existing.Seen = score.Seen;
        existing.Playing = score.Playing;
        existing.Maal = score.Maal;
        existing.BonusPoint = score.BonusPoint;
        existing.Duply = score.Duply;
        existing.Winner = score.Winner;
        existing.Score = score.Score;
        existing.MoneyWon = score.MoneyWon;
        existing.Deal = score.Deal;
        existing.Position = score.Position;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var score = await GetByIdAsync(id);
        if (score == null) return false;

        _context.MarriageGameScores.Remove(score);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.MarriageGameScores.AnyAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<MarriageGameScore>> GetByGameIdAsync(int gameId)
    {
        return await _context.MarriageGameScores
            .Where(s => s.MarriageGameId == gameId)
            .OrderBy(s => s.Position)
            .ToListAsync();
    }

    public async Task<IEnumerable<MarriageGameScore>> GetByPlayerIdAsync(Guid playerId)
    {
        return await _context.MarriageGameScores
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.MarriageGameId)
            .ToListAsync();
    }
}