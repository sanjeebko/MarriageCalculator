using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for GameSettings operations
/// </summary>
public interface IGameSettingsRepository
{
    Task<IEnumerable<GameSettings>> GetAllAsync();
    Task<IEnumerable<GameSettings>> GetByUserIdAsync(Guid userId);
    Task<GameSettings?> GetByIdAsync(int id);
    Task<GameSettings> CreateAsync(GameSettings settings);
    Task<GameSettings?> UpdateAsync(int id, GameSettings settings);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}