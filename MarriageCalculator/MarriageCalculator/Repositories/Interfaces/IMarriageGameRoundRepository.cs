using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGameRound operations in MAUI client
/// </summary>
public interface IMarriageGameRoundRepository
{
    Task<List<MarriageGameRound>> GetRoundsByGameSetIdAsync(int gameSetId);
    Task<MarriageGameRound?> GetRoundByIdAsync(int id);
    Task<MarriageGameRound> CreateRoundAsync(MarriageGameRound round);
    Task<MarriageGameRound?> UpdateRoundAsync(MarriageGameRound round);
    Task<bool> DeleteRoundAsync(int id);
}