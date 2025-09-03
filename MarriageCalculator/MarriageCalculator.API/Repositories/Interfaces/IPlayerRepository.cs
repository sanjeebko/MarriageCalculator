using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for Player operations
/// </summary>
public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<IEnumerable<Player>> GetByCreatorAsync(Guid userId);
    Task<Player?> GetByEmailAsync(string email);
    Task<Player?> GetByIdAsync(Guid id);
    Task<Player> CreateForUserAsync(Player player, Guid userId);
    Task<Player?> UpdateAsync(Guid id, Player player);
    Task<Player> SetCreatorAsync(Guid id, Guid userId);
    Task<Player> SetCreatorByUserIdAsync(Guid id, Guid userId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}