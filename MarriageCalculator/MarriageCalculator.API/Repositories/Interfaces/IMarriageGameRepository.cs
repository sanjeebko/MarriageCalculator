using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGame operations
/// </summary>
public interface IMarriageGameRepository
{
    Task<IEnumerable<MarriageGame>> GetAllAsync();
    Task<MarriageGame?> GetByIdAsync(int id);
    Task<MarriageGame> CreateAsync(MarriageGame game);
    Task<MarriageGame?> UpdateAsync(int id, MarriageGame game);
    Task<bool> DeleteGameAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<MarriageGame>> GetByRoundIdAsync(int roundId);
}