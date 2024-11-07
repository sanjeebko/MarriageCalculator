



namespace MarriageCalculator.Services;

public interface IMarriageGameEngine
{
    string LastPageName { get; set; }
    IDbService DbServices { get; }
    ISettingsService SettingsService { get; }
    IPlayerService PlayerService { get; }
    CancellationTokenSource CancellationTokenSource { get; }
    ITextToSpeechService TextToSpeechService { get; }
    bool Initialized { get; }
    Task InitializeEngineAsync();
}