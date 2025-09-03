using MarriageCalculator.Core.Models;
using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;
using System.Collections.Generic;

namespace MarriageCalculator.Services.Implementations;

public class MarriageGameEngine( 
                                IAuthenticationService authenticationService,
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
     
    public IAuthenticationService AuthenticationService { get; } = authenticationService;
    public ISettingsService SettingsService { get; } = settingsService;
    public IPlayerService PlayerService { get; } = playerService;

    public IMarriageGameSetRepository MarriageGameSetRepository { get; } = marriageGameSetRepository;
    public IMarriageGameRoundRepository MarriageGameRoundRepository { get; } = marriageGameRoundRepository;
    public IMarriageGameRepository MarriageGameRepository { get; } = marriageGameRepository;
    public IMarriageGameScoreRepository MarriageGameScoreRepository { get; } = marriageGameScoreRepository;
    public IMarriageGameSetPlayerRepository MarriageGameSetPlayerRepository { get; } = marriageGameSetPlayerRepository;
    public ITextToSpeechService TextToSpeechService { get; } = textToSpeechService;
    public bool Initialized { get; private set; } = false;
    public string LastPageName { get; set; } = nameof(MarriageGameEngine);
    public MarriageGameSet? MarriageGameSet { get; private set; }
    public MarriageGameRound? CurrentMarriageGameRound { get; private set; }
    public MarriageGame? CurrentMarriageGame { get; private set; }
    
    public bool IsServerConnected { get; private set; } = false;

    public async Task InitializeEngineAsync()
    {  
        if (Initialized && SettingsService.IsInitialized && PlayerService.IsInitialized) 
            return;
    
        var initializeSettingsServiceTask = SettingsService.InitializeAsync();
        var initializePlayerServiceTask = PlayerService.InitializeAsync();
        var initializeTextToSpeechServiceTask = TextToSpeechService.InitializeAsync();
        
        var initializeLastGameSetTask = InitializeLastGameSetAsync(); 

        await Task.WhenAll(initializePlayerServiceTask, initializeSettingsServiceTask, initializeTextToSpeechServiceTask, initializeLastGameSetTask);

        Initialized = true;
        IsServerConnected = true;
    }

    private async Task InitializeLastGameSetAsync()
    {
        MarriageGameSet = await MarriageGameSetRepository.GetLatestGameSetAsync();
        PlayerService.ActivePlayers.Clear();
        if (MarriageGameSet is not null)
        {
            var players = await MarriageGameSetPlayerRepository.GetPlayersByGameSetIdAsync(MarriageGameSet.Id);
            foreach (var player in players)
            {
                MarriageGameSet.GameSetPlayers.TryAdd(player.PlayerId, player);
                PlayerService.ActivePlayers.TryAdd(player.PlayerId, player.Player);
            }            
        }
        
        
    }

    public async Task CreateNewGameSet()
    {
        if (MarriageGameSet is not null && MarriageGameSet.IsActive)
        {
            await CloseCurrentGameSet();
        }

        var name = DateTime.UtcNow.ToString("yyyyMMdd HHmmss");
        var marriageGameSetTask = MarriageGameSetRepository.CreateGameSetAsync(new MarriageGameSet
        {
            Name = name,
            Created = DateTime.UtcNow,
            LastPlayed = DateTime.UtcNow,
            IsActive = true,
            GameSettingsId = SettingsService.Settings!.Id
        });

        if (SettingsService.Settings!.Id==0)
        {
            await SettingsService.SaveSettingsAsync();
            await SettingsService.LoadSettingsAsync();
        }
        var gameSettings = SettingsService.Settings;

        var marriageGameSet = await marriageGameSetTask ?? throw new Exception("Failed to create new game set");
        marriageGameSet.GameSettingsId = gameSettings.Id;
        marriageGameSet.GameSetPlayers = PlayerService.ActivePlayers.ToDictionary(
            player => player.Key,
            player => new MarriageGameSetPlayer { PlayerId = player.Key, MarriageGameSetId = marriageGameSet.Id }
        );

        await AddPlayersToGameSetAsync(marriageGameSet.GameSetPlayers);
        var updateGameSetTask = MarriageGameSetRepository.UpdateGameSetAsync(marriageGameSet);
        var createNewGameRoundTask = CreateNewGameRoundForGivenGameSet(marriageGameSet.Id);

        await Task.WhenAll(updateGameSetTask, createNewGameRoundTask);

        MarriageGameSet = marriageGameSet;
    }
    
    public async Task CreateNewGameRoundForGivenGameSet(int id)
    {
        var rounds = await MarriageGameRoundRepository.GetRoundsByGameSetIdAsync(id);
        var latestRound = rounds.OrderByDescending(x => x.Sequence).FirstOrDefault();
        int sequence = 1;
        if (latestRound is not null)
            sequence += latestRound.Sequence+1;

        var marriageGameRound = new MarriageGameRound { MarriageGameSetId = id, Sequence = sequence };
        await MarriageGameRoundRepository.CreateRoundAsync(marriageGameRound);
        await CreateNewMarriageGameForGivenGameRound(marriageGameRound);
        CurrentMarriageGameRound = marriageGameRound;
    }
    
    public async Task<MarriageGame> CreateNewMarriageGameForGivenGameRound(MarriageGameRound marriageGameRound)
    { 
        var sequence = 1;
        var allMarriageGames = await MarriageGameRepository.GetGamesByRoundIdAsync(marriageGameRound.Id);
        if(allMarriageGames.Count > 0)
            sequence = allMarriageGames.Max(x => x.Sequence) + 1;

        var marriageGame = new MarriageGame { MarriageGameRoundId = marriageGameRound.Id, Sequence = sequence, CreatedTime = DateTime.UtcNow };
        await MarriageGameRepository.CreateGameAsync(marriageGame);
        //add marriageGamescore to marriageGame
        int playerIndex = 0;
        foreach (var player in PlayerService.ActivePlayers)
        {
            playerIndex++;
            var marriageGameScore = new MarriageGameScore { PlayerId = player.Key, MarriageGameId = marriageGame.Id, MarriageGame = marriageGame, Position = playerIndex };

            await MarriageGameScoreRepository.CreateScoreAsync(marriageGameScore);

            marriageGame.MarriageGameScores
                .Add(player.Key,marriageGameScore );
        }

        CurrentMarriageGame = marriageGame;
        marriageGameRound.MarriageGames.Add(marriageGame);
        return marriageGame;
    }
    
    private async Task AddPlayersToGameSetAsync(Dictionary<Guid, MarriageGameSetPlayer> gameSetPlayers)
    {
        var existingPlayers = await MarriageGameSetPlayerRepository.GetPlayersByGameSetIdAsync(MarriageGameSet!.Id);

        // select all players from existingplayers whose PlayerId is not in new Players list
        var playersToRemove = existingPlayers.Where(ep => !gameSetPlayers.ContainsKey(ep.PlayerId)).ToList();
        var removeTasks = playersToRemove.Select(player => MarriageGameSetPlayerRepository.DeleteGameSetPlayerAsync(MarriageGameSet.Id, player.PlayerId));

        await Task.WhenAll(removeTasks);

        // get list of new players from gameSetPlayers whose PlayerId is not in existingPlayers
       var completelyNewGameSetPlayers = gameSetPlayers
            .Where(gp => !existingPlayers.Any(ep => ep.PlayerId == gp.Key))
            .ToDictionary(gp => gp.Key, gp => gp.Value);

        var tasks = completelyNewGameSetPlayers.Values.Select(player => MarriageGameSetPlayerRepository.CreateGameSetPlayerAsync(player));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    ///     Adds players to the marriage game set asynchronously.
    /// </summary>
    /// <remarks>This method retrieves the players for the marriage game set and adds them to the game
    /// set.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddMarriageGameSetPlayerAsync()
    {
        await AddPlayersToGameSetAsync(GetMarriageGameSetPlayers());
    }
    private Dictionary<Guid, MarriageGameSetPlayer> GetMarriageGameSetPlayers()
    {
        if (MarriageGameSet is null)
            return new Dictionary<Guid, MarriageGameSetPlayer>();

        // 
        DebugPlayerServiceState("testing active players!");

        return PlayerService.ActivePlayers.ToDictionary(
           player => player.Key,
           player => new MarriageGameSetPlayer { PlayerId = player.Key, MarriageGameSetId = MarriageGameSet.Id }
       );
    }
    private void DebugPlayerServiceState(string context)
    {
        System.Diagnostics.Debug.WriteLine($"=== PlayerService Debug - {context} ===");
        System.Diagnostics.Debug.WriteLine($"IsInitialized: {PlayerService.IsInitialized}");
        System.Diagnostics.Debug.WriteLine($"AllPlayers count: {PlayerService.AllPlayers?.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"ActivePlayers count: {PlayerService.ActivePlayers?.Count ?? 0}");

        if (PlayerService.ActivePlayers?.Count > 0)
        {
            foreach (var player in PlayerService.ActivePlayers)
            {
                System.Diagnostics.Debug.WriteLine($"  Active Player: {player.Key} - {player.Value?.Name ?? "No Name"}");
            }
        }
        System.Diagnostics.Debug.WriteLine("=====================================");
    }
    public async Task<bool> ResumePreviousGameIfAvailable()
    {
        var marriageGameSet = await MarriageGameSetRepository.GetLatestGameSetAsync();
        if (marriageGameSet is null)
            return false;

        var marriageGameRoundsTask = MarriageGameRoundRepository.GetRoundsByGameSetIdAsync(marriageGameSet.Id);
        var marriageGameSetPlayersTask = MarriageGameSetPlayerRepository.GetPlayersByGameSetIdAsync(marriageGameSet.Id); 
        var settingsTask = SettingsService.GetGameSettingsByIdAsync(marriageGameSet.GameSettingsId);

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

        var marriageGames = await MarriageGameRepository.GetGamesByRoundIdAsync(marriageGameRound.Id);
        var marriageGame = marriageGames.FirstOrDefault(x => x.WinnerId == Guid.Empty);

        if (marriageGame is null)
            return false;

        var marriageGameScores = await MarriageGameScoreRepository.GetScoresByGameIdAsync(marriageGame.Id);
        if (marriageGameScores is null || marriageGameScores.Count == 0)
        {
            if(marriageGameSet.GameSetPlayers.Count==0)
                return false;
        }
        marriageGame.MarriageGameScores = marriageGameScores?.ToDictionary(x => x.PlayerId, x => x)??[];

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
            await MarriageGameSetRepository.UpdateGameSetAsync(MarriageGameSet);
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
        var marriageGameSet = await MarriageGameSetRepository.GetLatestGameSetAsync();
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
            tasks.Add(MarriageGameScoreRepository.UpdateScoreAsync(marriageGameScore));
        }

        await Task.WhenAll(tasks);

        CurrentMarriageGame.TotalMaal = CurrentMarriageGame.MarriageGameScores.Values.Sum(x => x.Maal);
        var winnerId = CurrentMarriageGame.MarriageGameScores.Values.FirstOrDefault(x => x.Winner)?.PlayerId;
        if (winnerId.HasValue && winnerId.Value != Guid.Empty)
        {
            CurrentMarriageGame.WinnerId = winnerId.Value;
        }

        MarriageGameSet.LastPlayed = DateTime.UtcNow;
        tasks.Add(MarriageGameRepository.UpdateGameAsync(CurrentMarriageGame));
        tasks.Add(MarriageGameSetRepository.UpdateGameSetAsync(MarriageGameSet));
     
        await Task.WhenAll(tasks);
    }

    public async Task CloseCurrentGameRound()
    {
        await SaveCurrentGame();
        if (CurrentMarriageGameRound is not null)
        {
            CurrentMarriageGameRound.Completed = true;
            await MarriageGameRoundRepository.UpdateRoundAsync(CurrentMarriageGameRound);
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

    public bool IsPlayersReady => PlayerService.ActivePlayers.Count >= 2;
}
