namespace MarriageCalculator.Services.Interfaces;

public interface ISettingsService
{
    GameSettings?  Settings { get; set; }
    bool IsInitialized { get; }

    Task InitializeAsync();
    Task<GameSettings?> LoadSettingsAsync( );
    GameSettings? GetSettingsById(int settingsId);
    Task<GameSettings?> GetGameSettingsByIdAsync(int settingsId);
    Task SaveSettingsAsync( );
}