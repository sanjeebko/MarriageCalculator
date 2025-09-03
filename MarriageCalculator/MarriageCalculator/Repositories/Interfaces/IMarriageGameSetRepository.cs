using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories.Interfaces;

/// <summary>
/// Repository interface for MarriageGameSet operations in MAUI client
/// </summary>
public interface IMarriageGameSetRepository
{
    Task<List<MarriageGameSet>> GetAllGameSetsAsync();
    Task<MarriageGameSet?> GetGameSetByIdAsync(int id);
    Task<MarriageGameSet?> GetLatestGameSetAsync();
    Task<MarriageGameSet> CreateGameSetAsync(MarriageGameSet gameSet);
    Task<MarriageGameSet?> UpdateGameSetAsync(MarriageGameSet gameSet);
    Task<bool> DeleteGameSetAsync(int id);
}