using System.Text.Json;

namespace MarriageCalculator.Services;

public class SettingsService(IDbService dbService) : ISettingsService
{
     
    public GameSettings?  Settings { get; set; }
    public Dictionary<int, GameSettings> GameSettings { get; set; } = [];
    public IDbService DbService { get; } = dbService;

    public async Task InitializeAsync()
    {
        GameSettings = await DbService.GetAllGameSettingsAsync();
        var latestSettings = GameSettings.OrderByDescending(x => x.Key).FirstOrDefault();
        if(latestSettings.Value != null)
            Settings =  latestSettings.Value;
        else
            Settings = GetDefaultSettings();
    }
    
    ///<summary>
    /// Saves the current game settings asynchronously.
    /// Throws an exception if the settings are null.
    /// </summary> 
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveSettingsAsync( )
    {
        Settings ??= Core.Models.GameSettings.Default();
        await DbService.AddGameSettingsAsync(Settings);
    }

    public async Task<GameSettings> LoadSettingsAsync( )
    {
        var settings =await DbService.GetLastGameSettingsAsync();
        if (settings == null)
        {
            Settings = GetDefaultSettings();
            await DbService.AddGameSettingsAsync(Settings); 
            settings = Settings;
        } 

        Settings = settings;
        return Settings;
    }

   

public async Task<GameSettings?> GetSettingsByIdAsync(int settingsId)
    {
        if (GameSettings.TryGetValue(settingsId, out var settings))
        {
            return settings;
        }

        settings = await DbService.GetGameSettingsAsync(settingsId);
        if (settings != null)
        {
            Settings = settings;
        }
        return settings;
    }
    
    private GameSettings GetDefaultSettings()
    {
      return  Core.Models.GameSettings.Default(); 
    }

}
