using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGame operations in MAUI client
/// </summary>
public interface IMarriageGameRepository
{
    Task<List<MarriageGame>> GetAllGamesAsync();
    Task<List<MarriageGame>> GetGamesByRoundIdAsync(int roundId);
    Task<MarriageGame?> GetGameByIdAsync(int id);
    Task<MarriageGame> CreateGameAsync(MarriageGame game);
    Task<MarriageGame?> UpdateGameAsync(MarriageGame game);
    Task<bool> DeleteGameAsync(int id);
}