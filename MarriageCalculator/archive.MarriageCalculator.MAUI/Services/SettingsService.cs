using System.Text.Json;

namespace MarriageCalculator.Services;

public class SettingsService(IDbService dbService) : ISettingsService
{
     
    public GameSettings?  Settings { get; set; }
    public Dictionary<int, GameSettings> GameSettings { get; set; } = [];
    public IDbService DatabaseService { get; } = dbService;
    public bool IsInitialized { get; private set; } = false;
    public async Task InitializeAsync()
    {
        var isConnected = await DatabaseService.TestConnectionAsync();
        if (!isConnected)
        {
            IsInitialized = false;
            return;
        }
        
        GameSettings = await DatabaseService.GetAllGameSettingsAsync();
        var latestSettings = GameSettings.OrderByDescending(x => x.Key).FirstOrDefault();
        if(latestSettings.Value != null)
            Settings =  latestSettings.Value;
        else
            Settings = GetDefaultSettings();

        IsInitialized = true;
    }
    
    ///<summary>
    /// Saves the current game settings asynchronously.
    /// Throws an exception if the settings are null.
    /// </summary> 
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveSettingsAsync( )
    {
        Settings ??= Core.Models.GameSettings.Default();
        await DatabaseService.AddGameSettingsAsync(Settings);
    }

    public async Task<GameSettings> LoadSettingsAsync( )
    {
        var settings =await DatabaseService.GetLastGameSettingsAsync();
        if (settings == null)
        {
            Settings = GetDefaultSettings();
            await DatabaseService.AddGameSettingsAsync(Settings); 
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

        settings = await DatabaseService.GetGameSettingsAsync(settingsId);
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
