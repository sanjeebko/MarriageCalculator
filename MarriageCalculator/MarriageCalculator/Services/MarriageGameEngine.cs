using System.Reflection.Metadata.Ecma335;

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
        await InitializeLastGameSetAsync();
        var initializePlayerServiceTask = PlayerService.InitializeAsync();
        var initializeSettingsServiceTask = SettingsService.InitializeAsync();
        var initializeTextToSpeechServiceTask = TextToSpeechService.InitializeAsync();

        await Task.WhenAll(initializePlayerServiceTask, initializeSettingsServiceTask, initializeTextToSpeechServiceTask);

        Initialized = true;
    }

    private async Task InitializeLastGameSetAsync() {
        MarriageGameSet = await DbServices.GetLatestMarriageGameSetAsync();

    }

    public async Task CreateNewGameSet()
    {
        if (MarriageGameSet is not null && MarriageGameSet.IsActive)
        {
            await CloseCurrentGameSet();
        }

         
        var name = DateTime.UtcNow.ToString("yyyyMMdd HHmmss");
        var marriageGameSetTask = DbServices.AddNewMarriageGameSetAsync(name);

         
        if (SettingsService.Settings!.Id==0)
        {
            await SettingsService.SaveSettingsAsync();
            await SettingsService.LoadSettingsAsync();
        }
        var gameSettings = SettingsService.Settings;

        var marriageGameSet = await marriageGameSetTask ?? throw new Exception("Failed to create new game set");
        marriageGameSet.GameSettingsId = gameSettings.Id;
        marriageGameSet.GameSetPlayers = PlayerService.Players.ToDictionary(
            player => player.Key,
            player => new MarriageGameSetPlayer { PlayerId = player.Key, MarriageGameSetId = marriageGameSet.Id }
        );

        await AddPlayersToGameSet(marriageGameSet.GameSetPlayers);
        var updateGameSetTask = DbServices.UpdateMarriageGameSetAsync(marriageGameSet);
        var createNewGameRoundTask = CreateNewGameRoundForGivenGameSet(marriageGameSet.Id);

        await Task.WhenAll( updateGameSetTask, createNewGameRoundTask);

        MarriageGameSet = marriageGameSet;
    }
    public async Task CreateNewGameRoundForGivenGameSet(int id)
    {
        var rounds =await DbServices.GetMarriageGameRoundsByMarriageGameSetIdAsync(id);
        var latestRound = rounds.OrderByDescending(x => x.Sequence).FirstOrDefault();
        int sequence = 1;
        if (latestRound is not null)
            sequence += latestRound.Sequence+1;

        var marriageGameRound = new MarriageGameRound { MarriageGameSetId = id, Sequence = sequence };
        await DbServices.AddMarriageGameRoundAsync(marriageGameRound);
        await CreateNewMarriageGameForGivenGameRound(marriageGameRound);
        CurrentMarriageGameRound = marriageGameRound;

    }
    
    public async Task<MarriageGame> CreateNewMarriageGameForGivenGameRound(MarriageGameRound marriageGameRound)
    { 
        var sequence = 1;
        var allMarriageGames = await DbServices.GetMarriageGamesByRoundIdAsync(marriageGameRound.Id);
        if(allMarriageGames.Count > 0)
            sequence = allMarriageGames.Max(x => x.Sequence) + 1;

        var marriageGame = new MarriageGame { MarriageGameRoundId = marriageGameRound.Id, Sequence = sequence, CreatedTime = DateTime.UtcNow };
        await DbServices.AddMarriageGameAsync(marriageGame);
        //add marriageGamescore to marriageGame
        int playerIndex = 0;
        foreach (var player in PlayerService.Players)
        {
            playerIndex++;
            var marriageGameScore = new MarriageGameScore { PlayerId = player.Key, MarriageGameId = marriageGame.Id, MarriageGame = marriageGame, Position = playerIndex };

            await DbServices.AddMarriageGameScoreAsync(marriageGameScore);

            marriageGame.MarriageGameScores
                .Add(player.Key,marriageGameScore );


        }
        

        CurrentMarriageGame = marriageGame;
        marriageGameRound.MarriageGames.Add(marriageGame);
        return marriageGame;
    }

    private async Task AddPlayersToGameSet(Dictionary<int, MarriageGameSetPlayer> gameSetPlayers)
    {
        var tasks = gameSetPlayers.Values.Select(player => DbServices.AddMarriageGameSetPlayerAsync(player));
        await Task.WhenAll(tasks);
    }
    public async Task<bool> ResumePreviousGameIfAvailable()
    {
         
        var marriageGameSet = await DbServices.GetLatestMarriageGameSetAsync();
        if (marriageGameSet is null)
            return false;

        var marriageGameRoundsTask = DbServices.GetMarriageGameRoundsByMarriageGameSetIdAsync(marriageGameSet.Id);
        var marriageGameSetPlayersTask = DbServices.GetMarriageGameSetPlayersByMarriageGameSetIdAsync(marriageGameSet.Id); 
        var settingsTask =  SettingsService.GetSettingsByIdAsync(marriageGameSet.GameSettingsId);


        await Task.WhenAll(marriageGameRoundsTask, marriageGameSetPlayersTask, settingsTask);

        var marriageGameRounds = await marriageGameRoundsTask;
        var marriageGameSetPlayers = await marriageGameSetPlayersTask;
        var settings = await settingsTask;
        if (settings is null)
        { 
            settings = SettingsService.Settings??GameSettings.Default();
            await SettingsService.SaveSettingsAsync();
            marriageGameSet.GameSettingsId = settings.Id;
             
        }

        if (marriageGameRounds is null || marriageGameSetPlayers.Count == 0 || settings is null)
            return false;

        marriageGameSet.GameSetPlayers = marriageGameSetPlayers.ToDictionary(player=> player.PlayerId, player=>player);
        marriageGameSet.Rounds = marriageGameRounds;
        marriageGameSet.GameSettings = settings;
        MarriageGameSet = marriageGameSet;
        if(marriageGameSet.GameSetPlayers.Count > 0)
            PlayerService.SelectPlayerByIds([.. marriageGameSet.GameSetPlayers.Keys]);


        var marriageGameRound = marriageGameRounds.FirstOrDefault(x => x.Completed == false);
        if (marriageGameRound is null)
        {
            return false;
        }

        var marriageGames = await DbServices.GetMarriageGamesByRoundIdAsync(marriageGameRound.Id);
        var marriageGame = marriageGames.FirstOrDefault(x => x.WinnerId == 0);

        if (marriageGame is null)
            return false;

        var marriageGameScores = await DbServices.GetMarriageGameScoresByMarriageGameIdAsync(marriageGame.Id);
        if (marriageGameScores is null || marriageGameScores.Count == 0)
        {
            if(marriageGameSet.GameSetPlayers.Count==0)
                return false;
        }
        marriageGame.MarriageGameScores = marriageGameScores.ToDictionary(x => x.PlayerId, x => x);

       
        
        CurrentMarriageGameRound = marriageGameRound;
        CurrentMarriageGame = marriageGame;

        return true;
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
            await DbServices.UpdateMarriageGameSetAsync(MarriageGameSet);
        }
        ResetCurrentGameSet();
    }

    private void ResetCurrentGameSet()
    {
        MarriageGameSet = null;
        CurrentMarriageGameRound = null;
        CurrentMarriageGame = null;
    }

    public async Task CleanMarriageGameSet()
    {
        await DbServices.CleanMarriageGameSet();
        var marriageGameSet = await DbServices.GetLatestMarriageGameSetAsync();
        if (marriageGameSet is null)
            Console.WriteLine("No marriage GameSet available!");
        ResetCurrentGameSet();
        Initialized = false;
    }
    public async Task SaveCurrentGame()
    {
        if (CurrentMarriageGame is null || MarriageGameSet is null)
        {
            return;
        }
        List<Task> tasks = [];
        //Save all the MarriageGameScores
        foreach (var marriageGameScore in CurrentMarriageGame.MarriageGameScores.Values)
        {
            marriageGameScore.MarriageGameId = CurrentMarriageGame.Id;
            //Save marriageGameScore
            tasks.Add(DbServices.AddMarriageGameScoreAsync(marriageGameScore));
        }

        await Task.WhenAll(tasks);

        CurrentMarriageGame.TotalMaal = CurrentMarriageGame.MarriageGameScores.Values.Sum(x => x.Maal);
        var winnerId = CurrentMarriageGame.MarriageGameScores.Values.FirstOrDefault(x => x.Winner)?.PlayerId;
        if (winnerId is not null || winnerId > 0)
        {
            CurrentMarriageGame.WinnerId = winnerId.Value;
        }

        MarriageGameSet.LastPlayed = DateTime.UtcNow;
        tasks.Add(DbServices.UpdateMarriageGameAsync(CurrentMarriageGame));
        tasks.Add(DbServices.UpdateMarriageGameSetAsync(MarriageGameSet));
     
        await Task.WhenAll(tasks);
    }

    public async Task CloseCurrentGameRound()
    {
        await SaveCurrentGame();
        if (CurrentMarriageGameRound is not null)
        {
            CurrentMarriageGameRound.Completed = true;
            await DbServices.UpdateMarriageGameRoundAsync(CurrentMarriageGameRound);
        }         
    }

    public async Task CloseCurrentGameAsync(bool completed)
    {
        if(completed)
        {
            await SaveCurrentGame();
            CurrentMarriageGame = null;
            return;
        }
        CurrentMarriageGame = null;
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
