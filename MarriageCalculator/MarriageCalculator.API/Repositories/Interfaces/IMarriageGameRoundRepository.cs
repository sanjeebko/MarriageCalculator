using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGameRound operations
/// </summary>
public interface IMarriageGameRoundRepository
{
    Task<IEnumerable<MarriageGameRound>> GetAllAsync();
    Task<MarriageGameRound?> GetByIdAsync(int id);
    Task<MarriageGameRound> CreateAsync(MarriageGameRound round);
    Task<MarriageGameRound?> UpdateAsync(int id, MarriageGameRound round);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<MarriageGameRound>> GetByGameSetIdAsync(int gameSetId);
}