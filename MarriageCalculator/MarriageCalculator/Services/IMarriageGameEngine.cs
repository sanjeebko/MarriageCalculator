



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
    MarriageGameSet? MarriageGameSet { get; }
    bool IsPlayersReady { get; }
    bool IsActiveGame { get; }
    MarriageGame? CurrentMarriageGame { get; }
    MarriageGameRound? CurrentMarriageGameRound { get; }

    Task CreateNewGameSet();
    Task<bool> ResumePreviousGameIfAvailable();
    Task SaveGameSet();
    
    Task CloseCurrentGameSet();
    Task SaveCurrentGame();
    Task<MarriageGame> CreateNewMarriageGameForGivenGameRound(MarriageGameRound marriageGameRound);
    Task CloseCurrentGameRound();
    Task CloseCurrentGameAsync(bool completed);
    Task CreateNewGameRoundForGivenGameSet(int id);
    Task CleanMarriageGameSet();
}