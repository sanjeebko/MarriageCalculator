using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGameSetPlayer operations in MAUI client
/// </summary>
public interface IMarriageGameSetPlayerRepository
{
    Task<List<MarriageGameSetPlayer>> GetPlayersByGameSetIdAsync(int gameSetId);
    Task<MarriageGameSetPlayer?> GetGameSetPlayerByIdAsync(int gameSetId, Guid playerId);
    Task<MarriageGameSetPlayer> CreateGameSetPlayerAsync(MarriageGameSetPlayer gameSetPlayer);
    Task<bool> DeleteGameSetPlayerAsync(int GameSetId, Guid playerId);
}