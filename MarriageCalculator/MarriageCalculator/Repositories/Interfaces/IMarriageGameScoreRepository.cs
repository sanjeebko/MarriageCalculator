using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGameScore operations in MAUI client
/// </summary>
public interface IMarriageGameScoreRepository
{
    Task<List<MarriageGameScore>> GetScoresByGameIdAsync(int gameId);
    Task<MarriageGameScore?> GetScoreByIdAsync(int id);
    Task<MarriageGameScore> CreateScoreAsync(MarriageGameScore score);
    Task<MarriageGameScore?> UpdateScoreAsync(MarriageGameScore score);
    Task<bool> DeleteScoreAsync(int id);
    Task<List<MarriageGameScore>> GetScoresByPlayerIdAsync(Guid playerId);
}