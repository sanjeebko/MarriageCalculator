using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Services.Interfaces;

public interface IMarriageGameEngine
{
    string LastPageName { get; set; }
    //IDbService DatabaseService { get; }
    ISettingsService SettingsService { get; }
    IPlayerService PlayerService { get; }
    CancellationTokenSource CancellationTokenSource { get; }
    ITextToSpeechService TextToSpeechService { get; }
    bool Initialized { get; }
    Task InitializeEngineAsync();
    List<MarriageGameSet>? MarriageGameSets { get; }
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
    Task CreateNewMarriageGame();
    Task AddMarriageGameSetPlayerAsync();
    Task CloseCurrentGameRound();
    Task CloseCurrentGameAsync(bool completed);
    Task CreateNewGameRoundForGivenGameSet(int id);
    Task CleanMarriageGameSet();
    Task AddPlayerToGameSetAsync(MarriageGameSetPlayer player);
    Task RemovePlayerFromGameSetAsync(Guid playerId);
    Task RefreshPlayers();
    Task LoadGameSetAsync(int gameSetId);
    void SetServerConnectedStatus(bool isConnected);
    Task<List<MarriageGameSetPlayer>> GetGameSetPlayersByIdAsync(int id);
    void SetUserId(Guid userId);
}