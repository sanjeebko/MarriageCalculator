using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public PlayerRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        return await _context.Players.Where(p => !p.Deleted).ToListAsync();
    }

    public async Task<Player?> GetByIdAsync(int id)
    {
        return await _context.Players.FirstOrDefaultAsync(p => p.Id == id && !p.Deleted);
    }

    public async Task<Player> CreateAsync(Player player)
    {
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task<Player?> UpdateAsync(int id, Player player)
    {
        var existingPlayer = await GetByIdAsync(id);
        if (existingPlayer == null) return null;

        existingPlayer.Name = player.Name;
        existingPlayer.Email = player.Email;
        
        await _context.SaveChangesAsync();
        return existingPlayer;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var player = await GetByIdAsync(id);
        if (player == null) return false;

        player.Deleted = true; // Soft delete
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Players.AnyAsync(p => p.Id == id && !p.Deleted);
    }
}