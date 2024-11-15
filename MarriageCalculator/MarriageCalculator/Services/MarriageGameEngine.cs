
using System.Reflection;

namespace MarriageCalculator.Services;

public class MarriageGameEngine(IDbService dbServices, ISettingsService settingsService, IPlayerService playerService, ITextToSpeechService textToSpeechService) : IMarriageGameEngine
{

    /// <summary>
    /// MarriageGameEngine->MarriageGameSet->MarriageGameRound->MarriageGame
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
    public IDbService DbServices { get; } = dbServices;
    public ISettingsService SettingsService { get; } = settingsService;
    public IPlayerService PlayerService { get; } = playerService;
    public ITextToSpeechService TextToSpeechService { get; } = textToSpeechService;
    public bool Initialized { get; private set; } = false;
    public string LastPageName { get; set; } = nameof(MarriageGameEngine);
    public MarriageGameSet? MarriageGameSet { get; private set; }
    public MarriageGameRound? CurrentMarriageGameRound { get; private set; }
    public MarriageGame? CurrentMarriageGame { get; private set; }
    

    public async Task InitializeEngineAsync()
    { 
        await PlayerService.InitializeAsync();
        await SettingsService.InitializeAsync();
        await TextToSpeechService.InitializeAsync();
        await InitializeLastGameSetAsync();
        Initialized = true;
    }

    private async Task InitializeLastGameSetAsync() { 
        var marriageGameSet = await DbServices.GetLatestMarriageGameSetAsync();
        if(marriageGameSet is not null)
        {
            MarriageGameSet = marriageGameSet;
        }

    }

    public async Task CreateNewGameSet()
    {
        if (MarriageGameSet is not null && MarriageGameSet.IsActive)
        {
            await CloseCurrentGameSet();
        }
        var gameSettings = await DbServices.AddGameSettingsAsync(SettingsService.Settings);
        var marriageGameSet = await DbServices.AddNewMarriageGameSetAsync("") ?? throw new Exception("Failed to create new game set");

        marriageGameSet.GameSettingsId = gameSettings.Id;
        marriageGameSet.GameSetPlayers = PlayerService.Players.Select(player => new MarriageGameSetPlayer { PlayerId = player.Id, MarriageGameSetId = marriageGameSet.Id }).ToList();
        await AddPlayersToGameSet(marriageGameSet.Id);

        var marriageGameRound = new MarriageGameRound { MarriageGameSetId = marriageGameSet.Id, Sequence = 1 };
        marriageGameSet.Rounds.Add(marriageGameRound);

        await DbServices.UpdateMarriageGameSetAsync(marriageGameSet);
        await DbServices.AddMarriageGameRoundAsync(marriageGameRound);
        await CreateNewMarriageGameForGivenGameRound(marriageGameRound);

        MarriageGameSet = marriageGameSet;        
        CurrentMarriageGameRound = marriageGameRound;
    }

    public async Task<MarriageGame> CreateNewMarriageGameForGivenGameRound(MarriageGameRound marriageGameRound)
    {
        var sequence = 1;
        if(CurrentMarriageGame is not null)
        {
            sequence = CurrentMarriageGame.Sequence + 1;
        }
        var marriageGame = new MarriageGame { MarriageGameRoundId = marriageGameRound.Id, Sequence = sequence, CreatedTime = DateTime.UtcNow };
        await DbServices.AddMarriageGameAsync(marriageGame);
        //add marriageGamescore to marriageGame
        int playerIndex = 0;
        foreach (var player in PlayerService.Players)
        {
            playerIndex++;
            marriageGame.MarriageGameScores.Add(player.Id, new MarriageGameScore { PlayerId = player.Id, MarriageGame = marriageGame, Position = playerIndex });
        }
        
        CurrentMarriageGame = marriageGame;
        marriageGameRound.MarriageGames.Add(marriageGame);
        return marriageGame;
    }

    private async Task AddPlayersToGameSet(int marriageGameSetId)
    {
        foreach (var player in PlayerService.Players)
        {
            await DbServices.AddMarriageGameSetPlayerAsync(new MarriageGameSetPlayer { PlayerId = player.Id, MarriageGameSetId = marriageGameSetId });
        }
    }

    public async Task ResumeOldGameSet()
    {
        await Task.Delay(1000);
    }

    public async Task SaveGameSet()
    {
        await Task.Delay(1000);
    }
    public async Task CloseCurrentGameSet()
    {
        if (MarriageGameSet is not null)
        {
            MarriageGameSet.IsActive = false;
            await DbServices.CloseMarriageGameSet(MarriageGameSet);
        }
    }

    public async void SaveCurrentGame()
    {
         
        if (CurrentMarriageGame is not null)
        { 
            //Save all the MarriageGameScores
            foreach (var marriageGameScore in CurrentMarriageGame.MarriageGameScores.Values)
            {
                marriageGameScore.MarriageGameId = CurrentMarriageGame.Id;
                //Save marriageGameScore
                await DbServices.AddMarriageGameScoreAsync(marriageGameScore);
            }
        }
        CurrentMarriageGame.TotalMaal = CurrentMarriageGame.MarriageGameScores.Values.Sum(x => x.Maal);
        var winnerId = CurrentMarriageGame.MarriageGameScores.Values.FirstOrDefault(x => x.Winner)?.PlayerId;
        if (winnerId is not null || winnerId>0)
        {
            CurrentMarriageGame.WinnerId = winnerId.Value;
        }

        await DbServices.UpdateMarriageGameAsync(CurrentMarriageGame);

    }

    public bool IsActiveGame
    {
        get
        {
            if (MarriageGameSet is not null)
            {
                return MarriageGameSet.IsActive;
            }
            return false;
        }
    }

    public bool IsPlayersReady => PlayerService.Players.Count >= 2;
}
