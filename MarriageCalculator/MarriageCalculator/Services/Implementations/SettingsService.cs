using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;
using System.Threading.Tasks;


namespace MarriageCalculator.Services.Implementations;

public class SettingsService(IGameSettingsRepository settingsRepository) : ISettingsService
{

    public GameSettings? Settings { get; set; }
    public Dictionary<int, GameSettings> GameSettings { get; set; } = [];
    public bool IsInitialized { get; private set; } = false;
    public Guid UserId  { get; set; } = Guid.Empty;
    public async Task InitializeAsync(Guid userId)
    {
        try
        {
            var allSettings = await settingsRepository.GetAllGameSettingsAsync();
            if (allSettings is not null && allSettings.Count > 0)
            {
                GameSettings = allSettings.ToDictionary(s => s.Id, s => s);
            }
            else
            {
                // If no settings found, create default settings
                Settings = GetDefaultSettings();
                try
                {
                    var settings = await settingsRepository.CreateGameSettingsAsync(Settings);
                    if (settings is not null)
                    {
                        Settings = settings;
                        GameSettings[Settings.Id] = Settings;
                        IsInitialized = true;
                    }
                }
                catch (Exception ex)
                {
                    //  default settings failed, log the error or handle it as needed
                    System.Diagnostics.Debug.WriteLine($"Failed to create default settings: {ex.Message}");
                    
                    // Use local default settings as fallback
                    Settings = GetDefaultSettings();
                    GameSettings[0] = Settings; // Use 0 as temporary ID for local settings
                    IsInitialized = true;
                }
                return;
            }

            var latestSettings = GameSettings.OrderByDescending(x => x.Key).FirstOrDefault();
            Settings = latestSettings.Value;

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SettingsService.InitializeAsync failed: {ex.Message}");
            
            // If API call fails (e.g., due to authentication), use default settings
            Settings = GetDefaultSettings();
            GameSettings[0] = Settings; // Use 0 as temporary ID for local settings
            IsInitialized = true;
            
            // Re-throw if it's an authentication issue so the caller can handle it
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw;
            }
        }
    }

    ///<summary>
    /// Saves the current game settings asynchronously.
    /// Throws an exception if the settings are null.
    /// </summary> 
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveSettingsAsync()
    {
        if (Settings is null)
            return;

      Settings =   await settingsRepository.UpdateGameSettingsAsync(Settings);
    }

    public async Task<GameSettings?> LoadSettingsAsync()
    {
        await InitializeAsync(UserId);

        return Settings;
    }  

    public GameSettings? GetSettingsById(int settingsId)
    {
        if (GameSettings.TryGetValue(settingsId, out var settings))
        {
            return settings;
        }
        throw new KeyNotFoundException($"Settings with ID {settingsId} not found.");
    }

    public async Task<GameSettings?> GetGameSettingsByIdAsync(int settingsId)
    {
        if (GameSettings.TryGetValue(settingsId, out var settings))
        {
            return settings;
        }
        
        // If not in cache, try to get from repository
        var gameSettings = await settingsRepository.GetGameSettingsByIdAsync(settingsId);
        if (gameSettings != null)
        {
            GameSettings[settingsId] = gameSettings;
            return gameSettings;
        }
        
        return null;
    }
    /// <summary>
    /// Creates and returns new default game settings for a new game set.
    /// This method ALWAYS creates a new settings record in the database with auto-incremented ID.
    /// </summary>
    /// <returns>Newly created GameSettings with valid database ID</returns>
    public async Task<GameSettings?> GetDefaultSettingsForNewGameSet()
    {
        // Always create new default settings (with Id = 0, which will be auto-incremented by database)
        var newSettings = GetDefaultSettings();
        
        // Create the settings in the database - API will return the settings with auto-incremented ID
        try
        {
            var createdSettings = await settingsRepository.CreateGameSettingsAsync(newSettings);
            if (createdSettings != null && createdSettings.Id > 0)
            {
                // Add the newly created settings to our cache
                GameSettings[createdSettings.Id] = createdSettings;
                
                // Update current settings reference
                Settings = createdSettings;
                
                System.Diagnostics.Debug.WriteLine($"Created new default settings with ID: {createdSettings.Id}");
                return createdSettings;
            }
            else
            {
                throw new InvalidOperationException("API returned settings without a valid ID");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to create default settings: {ex.Message}");
            throw new InvalidOperationException("Cannot create game set without valid game settings. Please ensure the API is accessible and you are authenticated.", ex);
        }
    }

    private GameSettings GetDefaultSettings() => Core.Models.GameSettings.Default(UserId);

}
