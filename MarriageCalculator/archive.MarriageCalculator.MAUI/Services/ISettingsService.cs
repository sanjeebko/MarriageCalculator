

namespace MarriageCalculator.Services;

public interface ISettingsService
{
    GameSettings?  Settings { get; set; }

    Task InitializeAsync();
    Task<GameSettings> LoadSettingsAsync( );
    Task<GameSettings?> GetSettingsByIdAsync(int settingsId);
    Task SaveSettingsAsync( );
}