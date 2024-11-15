

namespace MarriageCalculator.Services;

public interface ISettingsService
{
    GameSettings  Settings { get; set; }

    Task InitializeAsync();
    Task<GameSettings> LoadSettingsAsync(CancellationToken cancellationToken);
 
    Task SaveSettingsAsync(CancellationToken cancellationToken);
}