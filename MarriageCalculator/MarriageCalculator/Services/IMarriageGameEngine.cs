



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
    Task ResumeOldGameSet();
    Task SaveGameSet();
    
    Task CloseCurrentGameSet();
    void SaveCurrentGame();
    Task<MarriageGame> CreateNewMarriageGameForGivenGameRound(MarriageGameRound marriageGameRound);
}