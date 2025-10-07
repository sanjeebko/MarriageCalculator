using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;
using MarriageCalculator.Helpers;

namespace MarriageCalculator.Services.Implementations;

public class MarriageGameEngine( 
                                ISettingsService settingsService,
                                IPlayerService playerService,
                                IMarriageGameSetRepository marriageGameSetRepository,
                                IMarriageGameRoundRepository marriageGameRoundRepository,
                                IMarriageGameRepository marriageGameRepository,
                                IMarriageGameScoreRepository marriageGameScoreRepository,
                                IMarriageGameSetPlayerRepository marriageGameSetPlayerRepository,
                                ITextToSpeechService textToSpeechService) : IMarriageGameEngine
{

    /// <summary>
    /// MarriageGameEngine->MarriageGameSet->MarriageGameRound->MarriageGame
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
    public Guid? UserId { get; set; } 
    public ISettingsService SettingsService { get; } = settingsService;
    public IPlayerService PlayerService { get; } = playerService;
    public ITextToSpeechService TextToSpeechService { get; } = textToSpeechService;
    public bool Initialized { get; private set; } = false;
    public bool IsActiveGame => MarriageGameSet is { IsActive: true };
    public string LastPageName { get; set; } = string.Empty;
    public List<MarriageGameSet> MarriageGameSets { get; set; } = [];
    public MarriageGameSet? MarriageGameSet { get; set; }
    public MarriageGameRound? CurrentMarriageGameRound => MarriageGameSet?.Rounds?.FirstOrDefault(r => !r.Completed);
    public MarriageGame? CurrentMarriageGame => CurrentMarriageGameRound?.MarriageGames.FirstOrDefault(g => g.ClosedRound == false && (g.WinnerId == null || g.WinnerId == Guid.Empty));
    public List<MarriageGameScore>? CurrentMarriageGameScores => CurrentMarriageGame?.MarriageGameScores;
    public bool IsServerConnected { get; private set; } = false;

    public async Task InitializeEngineAsync()
    {
        // Prevent re-initialization if already done
        if (Initialized)
            return;

        // Check server connection status before proceeding
        if (!IsServerConnected)
        {
            System.Diagnostics.Debug.WriteLine("MarriageGameEngine: Server not connected, skipping initialization.");
            return;
        }

        // Ensure dependent services are ready
        if (!SettingsService.IsInitialized && UserId is not null)
        {
            await SettingsService.InitializeAsync(UserId.Value);
        }

        var initializeTextToSpeechServiceTask = TextToSpeechService.InitializeAsync();        
        var initializePlayerServiceTask = PlayerService.InitializeAsync();
        var initializedGameSetTask =    InitializeMarriageGameSet();
        

        await Task.WhenAll(initializeTextToSpeechServiceTask, initializePlayerServiceTask, initializedGameSetTask);

        Initialized = true;
        System.Diagnostics.Debug.WriteLine("MarriageGameEngine: Engine initialized successfully.");
    }
    private async Task InitializeMarriageGameSet()
    {
        //step1: get All MarriageGameSets
        MarriageGameSets = await marriageGameSetRepository.GetAllGameSetsAsync();
        if (MarriageGameSets is null)
        {
            MarriageGameSets = [];
            return;
        }
         
    }
    public void SetUserId(Guid userId)
    {
        if (UserId != userId)
        {
            UserId = userId;             
            Initialized = false; // Mark as not initialized to allow re-initialization with new UserId
        }
    }
    public async Task<List<MarriageGameSetPlayer>> GetGameSetPlayersByIdAsync(int id) => await marriageGameSetPlayerRepository.GetPlayersByGameSetIdAsync(id);
    public async Task LoadGameSetAsync(int gameSetId)
    {
        //Step2: Get current MarriageGameSet if available.
        MarriageGameSet = MarriageGameSets
            .Where(m => m.Id == gameSetId)
            .FirstOrDefault();

        //Step3: If gameset is not available, then create new gameset;
        if (MarriageGameSet is null)
            await CreateNewGameSet();

        //Step4: if it can not create new gameset, there's something wrong so return uninitialized. 
        if (MarriageGameSet == null)
        {
            return;
        }

        // If available load Players selected for gameset
        var gamesetPlayers = await marriageGameSetPlayerRepository.GetPlayersByGameSetIdAsync(MarriageGameSet.Id);

        if (gamesetPlayers is not null && gamesetPlayers.Count > 0)
        {
            MarriageGameSet.GameSetPlayers = gamesetPlayers.ToDictionary(player => player.PlayerId,
                                                                         player => Convert(PlayerService.AllPlayers[player.PlayerId], MarriageGameSet));
        }
        var rounds = await marriageGameRoundRepository.GetRoundsByGameSetIdAsync(MarriageGameSet!.Id);
        MarriageGameSet.Rounds = rounds;

        //Foreach rounds I want to Load Marriage Game
         foreach(var round in MarriageGameSet.Rounds)
         {
             var games = await marriageGameRepository.GetGamesByRoundIdAsync(round.Id);
             round.MarriageGames = games;
             //Foreach game I want to Load Marriage Game Scores
             foreach(var game in round.MarriageGames)
             {
                 var scores = await marriageGameScoreRepository.GetScoresByGameIdAsync(game.Id);
                 game.MarriageGameScores = scores;
             }
        }


    } 

    private MarriageGameSetPlayer Convert(Player player, MarriageGameSet marriageGameSet)
    {
        return MarriageGameSetPlayerHelper.FromPlayer(player, marriageGameSet);
    }

    public async Task CreateNewGameSet()
    {
        System.Diagnostics.Debug.WriteLine("MarriageGameEngine: CreateNewGameSet called");

        //Create Default Settings and assign default settings to the new gameset. 
        var defaultSettings = await SettingsService.GetDefaultSettingsForNewGameSet();
        if (defaultSettings is null || defaultSettings.Id == 0)
            throw new Exception("Cannot create game set without valid game settings. Please ensure the API is accessible and you are authenticated.");

        var name = DateTime.UtcNow.ToString("yyyyMMdd HHmmss");
        var marriageGameSetTask = marriageGameSetRepository.CreateGameSetAsync(new MarriageGameSet
        {
            Name = name,
            Created = DateTime.UtcNow,
            LastPlayed = DateTime.UtcNow,
            IsActive = true,
            GameSettingsId = defaultSettings.Id,
            GameSettings  = defaultSettings
        });


        var marriageGameSet = await marriageGameSetTask ?? throw new Exception("Failed to create new game set");
        MarriageGameSet = marriageGameSet;
        MarriageGameSets.Add(marriageGameSet);
        System.Diagnostics.Debug.WriteLine($"MarriageGameEngine: Created new game set with ID {marriageGameSet.Id}");
    }

    public async Task CreateNewGameRoundForGivenGameSet(int id)
    {
        // Validate that we have an active game set and it matches the provided id
        if (MarriageGameSet == null)
        {
            throw new InvalidOperationException("No active game set available. Please create or load a game set first.");
        }

        if (MarriageGameSet.Id != id)
        {
            throw new UnauthorizedAccessException("Cannot create round for a game set that is not currently active.");
        }

        var rounds = await marriageGameRoundRepository.GetRoundsByGameSetIdAsync(id);
        var latestRound = rounds.OrderByDescending(x => x.Sequence).FirstOrDefault();
        int sequence = 1;
        if (latestRound is not null)
            sequence = latestRound.Sequence + 1;

        var marriageGameRoundObject = new MarriageGameRound { MarriageGameSetId = id, Sequence = sequence };
        var marriageGameRound = await marriageGameRoundRepository.CreateRoundAsync(marriageGameRoundObject);

        if (MarriageGameSet.Rounds is null)
            MarriageGameSet.Rounds = [];

        if (!MarriageGameSet.Rounds.Any(a => a.Id == marriageGameRound.Id))
            MarriageGameSet.Rounds.Add(marriageGameRound);

        await CreateNewMarriageGame();
    }
    
    public async Task CreateNewMarriageGame()
    { 
        if(MarriageGameSet is null)
            throw new Exception("No active MarriageGameSet available!");
        if (MarriageGameSet.GameSetPlayers.Count < 2)
                throw new Exception("At least two players are required to start a new game!");
        if(CurrentMarriageGameRound is null)
            throw new Exception("No active MarriageGameRound available! Please create a new round first.");
        
        var marriageGameRound = CurrentMarriageGameRound;
        var sequence = 1;
        var allMarriageGames = await marriageGameRepository.GetGamesByRoundIdAsync(marriageGameRound.Id);
        if(allMarriageGames.Count > 0)
            sequence = allMarriageGames.Max(x => x.Sequence) + 1;

        var marriageGameObject = new MarriageGame { MarriageGameRoundId = marriageGameRound.Id, Sequence = sequence, CreatedTime = DateTime.UtcNow };
       var marriageGame = await marriageGameRepository.CreateGameAsync(marriageGameObject);
        //add marriageGamescore to marriageGame
        await CreateMarriageGameScoresForGameAsync(marriageGame);

        CurrentMarriageGameRound!.MarriageGames.Add(marriageGame);         
    }

    private async Task CreateMarriageGameScoresForGameAsync(MarriageGame marriageGame)
    {
        if (MarriageGameSet is null)
            throw new Exception("No active MarriageGameSet available!");
        if (MarriageGameSet.GameSetPlayers.Count < 2)
            throw new Exception("At least two players are required to start a new game!");
        marriageGame.MarriageGameScores ??= [];
        int playerIndex = 0;
        foreach (var player in MarriageGameSet.GameSetPlayers)
        {
            playerIndex++;
            var marriageGameScoreObject = new MarriageGameScore { PlayerId = player.Key, MarriageGameId = marriageGame.Id, MarriageGame = marriageGame, Position = playerIndex };
            var marriageGameScore = await marriageGameScoreRepository.CreateScoreAsync(marriageGameScoreObject);
            marriageGame.MarriageGameScores.Add(marriageGameScore);
        }
    }

    private async Task AddPlayersToGameSetAsync(Dictionary<Guid, MarriageGameSetPlayer> gameSetPlayers)
    {
        var existingPlayers = await marriageGameSetPlayerRepository.GetPlayersByGameSetIdAsync(MarriageGameSet!.Id);

        // select all players from existingplayers whose PlayerId is not in new Players list
        var playersToRemove = existingPlayers.Where(ep => !gameSetPlayers.ContainsKey(ep.PlayerId)).ToList();
        var removeTasks = playersToRemove.Select(player => marriageGameSetPlayerRepository.DeleteGameSetPlayerAsync(MarriageGameSet.Id, player.PlayerId));

        await Task.WhenAll(removeTasks);

        // get list of new players from gameSetPlayers whose PlayerId is not in existingPlayers
       var completelyNewGameSetPlayers = gameSetPlayers
            .Where(gp => !existingPlayers.Any(ep => ep.PlayerId == gp.Key))
            .ToDictionary(gp => gp.Key, gp => gp.Value);

        var tasks = completelyNewGameSetPlayers.Values.Select(player => marriageGameSetPlayerRepository.CreateGameSetPlayerAsync(player));
        await Task.WhenAll(tasks);
    }

    public async Task AddPlayerToGameSetAsync(MarriageGameSetPlayer player)
    {
        await marriageGameSetPlayerRepository.CreateGameSetPlayerAsync(player);
    }
    public async Task RemovePlayerFromGameSetAsync(Guid playerId)
    {
        if (MarriageGameSet is not null)
        { 
            await marriageGameSetPlayerRepository.DeleteGameSetPlayerAsync(MarriageGameSet.Id, playerId);
            if (MarriageGameSet.GameSetPlayers.ContainsKey(playerId))
                MarriageGameSet.GameSetPlayers.Remove(playerId);

        }
    }

    /// <summary>
    ///     Synchronizes the current marriage game set players with the database.
    /// </summary>
    /// <remarks>This method updates the database to match the current GameSetPlayers collection,
    /// adding new players and removing players that are no longer in the game set.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddMarriageGameSetPlayerAsync()
    {
        await AddPlayersToGameSetAsync(MarriageGameSet!.GameSetPlayers);
    }

    public async Task<bool> ResumePreviousGameIfAvailable()
    {
        var marriageGameSet = MarriageGameSet;
        if (marriageGameSet is null)
            return false;
        if (marriageGameSet.GameSetPlayers.Count == 0)
            return false;
         
        var marriageGameRounds = marriageGameSet.Rounds;
        var marriageGameSetPlayers = marriageGameSet.GameSetPlayers;
        var settings = SettingsService.Settings ?? throw new Exception("Game settings not found for the current game set.");

        if (marriageGameRounds is null || marriageGameSetPlayers.Count == 0 || settings is null)
            return false;

        marriageGameSet.GameSetPlayers = marriageGameSetPlayers ;
        marriageGameSet.Rounds = marriageGameRounds;
        marriageGameSet.GameSettings = settings;
        MarriageGameSet = marriageGameSet;


        var marriageGameRound = marriageGameRounds.FirstOrDefault(x => x.Completed == false);
        if (marriageGameRound is null)
        {
            return false;
        }

        var marriageGames = marriageGameRound.MarriageGames;
        var marriageGame = marriageGames.FirstOrDefault(x => x.ClosedRound == false && (x.WinnerId == null || x.WinnerId == Guid.Empty));

        if (marriageGame is null)
            return false;

        var marriageGameScores = marriageGame.MarriageGameScores; // await marriageGameScoreRepository.GetScoresByGameIdAsync(marriageGame.Id);
        if (marriageGameScores is null || marriageGameScores.Count == 0)
        {
            await CreateMarriageGameScoresForGameAsync(marriageGame);
        }
         

        return true;
    }
    public async Task<bool> ResumePreviousGameIfAvailable_old()
    {
        var marriageGameSet = await marriageGameSetRepository.GetLatestGameSetAsync();
        if (marriageGameSet is null)
            return false;
        if (marriageGameSet.GameSetPlayers.Count == 0)
            return false;
        var marriageGameRoundsTask = marriageGameRoundRepository.GetRoundsByGameSetIdAsync(marriageGameSet.Id);
        var marriageGameSetPlayersTask = marriageGameSetPlayerRepository.GetPlayersByGameSetIdAsync(marriageGameSet.Id); 
        var settingsTask = SettingsService.GetGameSettingsByIdAsync(marriageGameSet.GameSettingsId);

        await Task.WhenAll(marriageGameRoundsTask, marriageGameSetPlayersTask, settingsTask);

        var marriageGameRounds = await marriageGameRoundsTask;
        var marriageGameSetPlayers = await marriageGameSetPlayersTask;
        var settings = await settingsTask;
        if (settings is null)
        { 
            settings = SettingsService.Settings??GameSettings.Default(UserId!.Value);
            await SettingsService.SaveSettingsAsync();
            marriageGameSet.GameSettingsId = settings.Id;
        }

        if (marriageGameRounds is null || marriageGameSetPlayers.Count == 0 || settings is null)
            return false;

        marriageGameSet.GameSetPlayers = marriageGameSetPlayers.ToDictionary(player=> player.PlayerId, player=>player);
        marriageGameSet.Rounds = marriageGameRounds;
        marriageGameSet.GameSettings = settings;
        MarriageGameSet = marriageGameSet;
         

        var marriageGameRound = marriageGameRounds.FirstOrDefault(x => x.Completed == false);
        if (marriageGameRound is null)
        {
            return false;
        }

        var marriageGames = await marriageGameRepository.GetGamesByRoundIdAsync(marriageGameRound.Id);
        var marriageGame = marriageGames.FirstOrDefault(x => x.ClosedRound == false && (x.WinnerId == null || x.WinnerId == Guid.Empty));

        if (marriageGame is null)
            return false;

        var marriageGameScores = await marriageGameScoreRepository.GetScoresByGameIdAsync(marriageGame.Id);
        if (marriageGameScores is null || marriageGameScores.Count == 0)
        {
            await CreateMarriageGameScoresForGameAsync(marriageGame);
        }
        else
        {
            marriageGame.MarriageGameScores = marriageGameScores;
        }
               
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
            await marriageGameSetRepository.UpdateGameSetAsync(MarriageGameSet);
        }
        ResetCurrentGameSet();
    }

    private void ResetCurrentGameSet()
    {
        MarriageGameSet = null;        
    }

    public async Task CleanMarriageGameSet()
    {
        var marriageGameSet = await marriageGameSetRepository.GetLatestGameSetAsync();
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
        foreach (var marriageGameScore in CurrentMarriageGame.MarriageGameScores)
        {
            marriageGameScore.MarriageGameId = CurrentMarriageGame.Id;
            //Save marriageGameScore
            tasks.Add(marriageGameScoreRepository.UpdateScoreAsync(marriageGameScore));
        }

        await Task.WhenAll(tasks);

        CurrentMarriageGame.TotalMaal = CurrentMarriageGame.MarriageGameScores.Sum(x => x.Maal);
        var winnerId = CurrentMarriageGame.MarriageGameScores.FirstOrDefault(x => x.Winner)?.PlayerId;
        if (winnerId.HasValue && winnerId.Value != Guid.Empty)
        {
            CurrentMarriageGame.WinnerId = winnerId.Value;
        }

        MarriageGameSet.LastPlayed = DateTime.UtcNow;
        tasks.Add(marriageGameRepository.UpdateGameAsync(CurrentMarriageGame));
        tasks.Add(marriageGameSetRepository.UpdateGameSetAsync(MarriageGameSet));
     
        await Task.WhenAll(tasks);
    }

    public async Task CloseCurrentGameRound()
    {
        await SaveCurrentGame();
        if (CurrentMarriageGameRound is not null)
        {
            CurrentMarriageGameRound.Completed = true;
            await marriageGameRoundRepository.UpdateRoundAsync(CurrentMarriageGameRound);
        }         
    }

    public async Task CloseCurrentGameAsync(bool completed)
    {
        if (completed)
        {
            await SaveCurrentGame();
            return;
        }

    }

    public async Task RefreshPlayers()
    {
        var players = await marriageGameSetPlayerRepository.GetPlayersByGameSetIdAsync(MarriageGameSet!.Id);
        if (players is not null)
        {
            MarriageGameSet!.GameSetPlayers = players.ToDictionary(player => player.PlayerId, player => player);
        }
    }

   

    public bool IsPlayersReady => MarriageGameSet?.GameSetPlayers.Count >= 2;

    public void SetServerConnectedStatus(bool isConnected)
    {
        IsServerConnected = isConnected;
        System.Diagnostics.Debug.WriteLine($"MarriageGameEngine: Server connection status set to {IsServerConnected}");
    }

   
}
