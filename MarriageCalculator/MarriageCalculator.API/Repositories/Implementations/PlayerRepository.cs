using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class PlayerRepository : IPlayerRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public PlayerRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        return await _context.Players.Where(p => !p.Deleted).OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<IEnumerable<Player>> GetByCreatorAsync(Guid userId)
    {
        return await _context.Players
            .Where(p => !p.Deleted && p.CreatedByUserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Player?> GetByEmailAsync(string email)
    {
        var e = email?.Trim().ToLower() ?? string.Empty;
        if (string.IsNullOrEmpty(e)) return null;
        return await _context.Players
            .Where(p => !p.Deleted && p.Email != null && p.Email.Equals(e, StringComparison.CurrentCultureIgnoreCase))
            .FirstOrDefaultAsync();
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        return await _context.Players.FirstOrDefaultAsync(p => p.Id == id && !p.Deleted);
    }

    public async Task<Player> CreateForUserAsync(Player player, Guid userId)
    {
        // Ensure the player has the CreatedByUserId set
        player.CreatedByUserId = userId;
        
        // Check if user exists, if not, this might be a timing issue during user creation
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            // Log the issue but don't throw - maybe the user is in the process of being created
            Console.WriteLine($"Warning: User {userId} not found when creating player. Player will be created with user association anyway.");
        }

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task<Player?> UpdateAsync(Guid id, Player player)
    {
        var existingPlayer = await GetByIdAsync(id);
        if (existingPlayer == null) return null;

        existingPlayer.Name = player.Name;
        existingPlayer.Email = player.Email;
        
        await _context.SaveChangesAsync();
        return existingPlayer;
    }

    public async Task<Player> SetCreatorAsync(Guid id, Guid userId)
    {
        var existingPlayer = await GetByIdAsync(id) ?? throw new InvalidOperationException("Player not found");
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists) throw new InvalidOperationException("User not found");
        existingPlayer.CreatedByUserId = userId;
        await _context.SaveChangesAsync();
        return existingPlayer;
    }

    public async Task<Player> SetCreatorByUserIdAsync(Guid id, Guid userId)
    {
        return await SetCreatorAsync(id, userId);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var player = await GetByIdAsync(id);
        if (player == null) return false;

        player.Deleted = true; // Soft delete
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Players.AnyAsync(p => p.Id == id && !p.Deleted);
    }
}