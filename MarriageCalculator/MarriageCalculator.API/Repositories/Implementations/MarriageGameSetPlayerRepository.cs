using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class MarriageGameSetPlayerRepository : IMarriageGameSetPlayerRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public MarriageGameSetPlayerRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MarriageGameSetPlayer>> GetAllAsync()
    {
        return await _context.MarriageGameSetPlayers
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MarriageGameSetPlayer?> GetByIdAsync(int gameSetId, Guid playerId)
    {
        return await _context.MarriageGameSetPlayers
            .AsNoTracking()
            .FirstOrDefaultAsync(gsp => gsp.MarriageGameSetId == gameSetId && gsp.PlayerId == playerId);
    }

    public async Task<IEnumerable<MarriageGameSetPlayer>> GetByGameSetIdAsync(int gameSetId)
    {
        return await _context.MarriageGameSetPlayers
            .AsNoTracking()
            .Include(gsp => gsp.Player)
            .Where(gsp => gsp.MarriageGameSetId == gameSetId)
            .ToListAsync();
    }

    public async Task<IEnumerable<MarriageGameSetPlayer>> GetByPlayerIdAsync(Guid playerId)
    {
        return await _context.MarriageGameSetPlayers
            .AsNoTracking()
            .Where(gsp => gsp.PlayerId == playerId)
            .ToListAsync();
    }

    public async Task<MarriageGameSetPlayer> CreateAsync(MarriageGameSetPlayer gameSetPlayer)
    {
        _context.MarriageGameSetPlayers.Add(gameSetPlayer);
        await _context.SaveChangesAsync();
        return gameSetPlayer;
    }

    public async Task<bool> DeleteAsync(int gameSetId, Guid playerId)
    {
        var gameSetPlayer = await _context.MarriageGameSetPlayers
            .FirstOrDefaultAsync(gsp => gsp.MarriageGameSetId == gameSetId && gsp.PlayerId == playerId);
        if (gameSetPlayer == null) return false;

        _context.MarriageGameSetPlayers.Remove(gameSetPlayer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByGameSetIdAsync(int gameSetId)
    {
        var gameSetPlayers = await _context.MarriageGameSetPlayers
            .Where(gsp => gsp.MarriageGameSetId == gameSetId)
            .ToListAsync();
        if (!gameSetPlayers.Any()) return false;

        _context.MarriageGameSetPlayers.RemoveRange(gameSetPlayers);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int gameSetId, Guid playerId)
    {
        return await _context.MarriageGameSetPlayers
            .AsNoTracking()
            .AnyAsync(gsp => gsp.MarriageGameSetId == gameSetId && gsp.PlayerId == playerId);
    }
}