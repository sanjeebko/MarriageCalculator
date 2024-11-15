using System.Text.Json;

namespace MarriageCalculator.Services;

public class SettingsService : ISettingsService
{
    private const string SettingsKey = "GameSettings";
    public GameSettings  Settings { get; set; }

    public SettingsService()
    {
         
    }
    public async Task InitializeAsync()
    {
        Settings = await LoadSettingsAsync(CancellationToken.None);
    }
    public async Task SaveSettingsAsync(CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(Settings);
        await Task.Run(() => Preferences.Set(SettingsKey, json), cancellationToken);
    }

    public async Task<GameSettings> LoadSettingsAsync(CancellationToken cancellationToken)
    {
        var json = await Task.Run(() => Preferences.Get(SettingsKey, string.Empty), cancellationToken);
        if (string.IsNullOrEmpty(json))
            return GetDefaultSettings();
        var settings = JsonSerializer.Deserialize<GameSettings>(json);
        Settings = settings ?? GetDefaultSettings();
        return Settings;
    }

    private GameSettings GetDefaultSettings()
    {
        Settings ??= new GameSettings();

        Settings.Id = 0;
        
        Settings.Murder = true;
        Settings.Kidnap = false;
        Settings.SeenPoint = 10;
        Settings.UnseenPoint = 5;
        Settings.PointRate = 10;
        Settings.Currency = Currency.NPR_Rupee;
        Settings.Dublee = false;
        Settings.DubleePointLess = true;
        Settings.DubleePointBonus = 5;
        Settings.FoulPoint = 15;
        Settings.FoulPointBonus = FoulPointBonusType.THIS_GAME;

        return Settings;
    }
}
