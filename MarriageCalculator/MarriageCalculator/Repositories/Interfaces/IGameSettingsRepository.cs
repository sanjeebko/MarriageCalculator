using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories.Interfaces;

/// <summary>
/// Repository interface for GameSettings operations in MAUI client
/// </summary>
public interface IGameSettingsRepository
{
    Task<List<GameSettings>> GetAllGameSettingsAsync();
    Task<GameSettings?> GetGameSettingsByIdAsync(int id);
    Task<GameSettings?> GetLatestGameSettingsAsync();
    Task<GameSettings> CreateGameSettingsAsync(GameSettings gameSettings);
    Task<GameSettings?> UpdateGameSettingsAsync(GameSettings gameSettings);
    Task<bool> DeleteGameSettingsAsync(int id);
}