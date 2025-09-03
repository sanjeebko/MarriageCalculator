using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGameScore operations
/// </summary>
public interface IMarriageGameScoreRepository
{
    Task<IEnumerable<MarriageGameScore>> GetAllAsync();
    Task<MarriageGameScore?> GetByIdAsync(int id);
    Task<MarriageGameScore> CreateAsync(MarriageGameScore score);
    Task<MarriageGameScore?> UpdateAsync(int id, MarriageGameScore score);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<MarriageGameScore>> GetByGameIdAsync(int gameId);
    Task<IEnumerable<MarriageGameScore>> GetByPlayerIdAsync(Guid playerId);
}