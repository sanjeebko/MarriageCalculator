using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories.Interfaces;

/// <summary>
/// Repository interface for Player operations in MAUI client
/// </summary>
public interface IPlayerRepository
{
    Task<List<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerByIdAsync(Guid id);
    Task<Player> CreatePlayerAsync(Player player);
    Task<Player?> UpdatePlayerAsync(Player player);
    Task<bool> DeletePlayerAsync(Guid id);
    Task<Player?> EnsureCurrentUserPlayerAsync();
}