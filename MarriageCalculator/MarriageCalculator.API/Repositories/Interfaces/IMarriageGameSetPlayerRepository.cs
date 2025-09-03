using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGameSetPlayer operations
/// </summary>
public interface IMarriageGameSetPlayerRepository
{
    Task<MarriageGameSetPlayer> CreateAsync(MarriageGameSetPlayer gameSetPlayer);
    Task<bool> DeleteAsync(int gameSetId, Guid playerId);
    Task<bool> DeleteByGameSetIdAsync(int gameSetId);
    Task<bool> ExistsAsync(int gameSetId, Guid playerId);
    Task<IEnumerable<MarriageGameSetPlayer>> GetAllAsync();
    Task<IEnumerable<MarriageGameSetPlayer>> GetByGameSetIdAsync(int gameSetId);
    Task<MarriageGameSetPlayer?> GetByIdAsync(int gameSetId, Guid playerId);
    Task<IEnumerable<MarriageGameSetPlayer>> GetByPlayerIdAsync(Guid playerId);
}