using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGameSet operations
/// </summary>
public interface IMarriageGameSetRepository
{
    Task<IEnumerable<MarriageGameSet>> GetAllAsync();
    Task<IEnumerable<MarriageGameSet>> GetByGameSettingsIdAsync(int gameSettingsId);
    Task<MarriageGameSet?> GetByIdAsync(int id);
    Task<MarriageGameSet> CreateAsync(MarriageGameSet gameSet);
    Task<MarriageGameSet?> UpdateAsync(int id, MarriageGameSet gameSet);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<MarriageGameSet?> GetLatestActiveAsync();
    Task<MarriageGameSet?> GetLatestActiveForUserAsync(Guid userId);
    Task<MarriageGameSet?> GetActiveByGameSettingsIdAsync(int gameSettingsId);
}