
namespace MarriageCalculator.Services.Interfaces;

public interface ISettingsService
{
    GameSettings? Settings { get; set; }
    bool IsInitialized { get; }
    Task InitializeAsync(Guid userId);
    Task<GameSettings?> LoadSettingsAsync();
    GameSettings? GetSettingsById(int settingsId);
    Task<GameSettings?> GetGameSettingsByIdAsync(int settingsId);
    Task SaveSettingsAsync(); 
    Task<GameSettings?> GetDefaultSettingsForNewGameSet();
}
