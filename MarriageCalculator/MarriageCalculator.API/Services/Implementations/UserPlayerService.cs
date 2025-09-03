using Bogus;
using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class UserPlayerService : IUserPlayerService
{
    private readonly MarriageCalculatorDbContext _context;
    private readonly ILogger<UserPlayerService> _logger;

    public UserPlayerService(MarriageCalculatorDbContext context, ILogger<UserPlayerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates initial player when a user's email is verified:
    /// 1. One player representing this user (with email - app user)
    /// Note: These players are created BY the user but don't BELONG to the user
    /// </summary>
    public async Task CreateDefaultPlayerForUserAsync(User user)
    {
        try
        {
             
            // 1. Always create a player representing this user in games (has app account)
            // This is called during email verification, so we should create the default player
            var userPlayer = new Player
            {
                Name = user.DisplayName,
                Email = user.Email,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Deleted = false,
                Selected = false
            };
            
             
            _logger.LogInformation("Creating default user player '{PlayerName}' for user {UserId} during email verification", userPlayer.Name, user.Id);
             
            
            // 2. Save all players
            await _context.Players.AddAsync(userPlayer);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Successfully created default player for user {UserId}: 1 default player '{PlayerName}'.", 
                user.Id, userPlayer.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default player for user {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Gets all available players for game selection
    /// </summary>
    public async Task<List<Player>> GetAllAvailablePlayersAsync()
    {
        return await _context.Players
            .Where(p => !p.Deleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Finds a player by their email address (app users only)
    /// </summary>
    public async Task<Player?> FindPlayerByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;
            
        return await _context.Players
            .Where(p => !string.IsNullOrEmpty(p.Email) && p.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase) && !p.Deleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all guest players (those without app accounts/email addresses)
    /// </summary>
    public async Task<List<Player>> GetGuestPlayersAsync()
    {
        return await _context.Players
            .Where(p => string.IsNullOrEmpty(p.Email) && !p.Deleted)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all app users (players with email addresses/app accounts)
    /// </summary>
    public async Task<List<Player>> GetAppUsersAsync()
    {
        return await _context.Players
            .Where(p => !string.IsNullOrEmpty(p.Email) && !p.Deleted)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new player (requires createdByUserId - all players must be created by a user)
    /// </summary>
    public async Task<Player> CreatePlayerAsync(string name, string email, Guid createdByUserId)
    {
        var player = new Player
        {
            Name = name.Trim(),
            Email = email?.Trim() ?? "",
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            Deleted = false,
            Selected = false
        };
        
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        
        var playerType = string.IsNullOrEmpty(player.Email) ? "guest" : "app user";
        _logger.LogInformation("Created {PlayerType} player: {PlayerName} (Email: {Email}) by user {UserId}", 
            playerType, player.Name, string.IsNullOrEmpty(player.Email) ? "None" : player.Email, createdByUserId);
        
        return player;
    }
}