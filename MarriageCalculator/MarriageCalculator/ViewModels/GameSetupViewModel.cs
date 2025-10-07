using CommunityToolkit.Mvvm.ComponentModel;
using MarriageCalculator.Pages.Game;
using MarriageCalculator.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MarriageCalculator.ViewModels;

 
public partial class GameSetupViewModel : ObservableObject
{
    private readonly IMarriageGameEngine _gameEngine;
    public IMarriageGameEngine GameEngine => _gameEngine;

    private readonly IApiService _apiService;
    
    [ObservableProperty]
    private bool isBusy;
    
 
    
    [ObservableProperty]
    private string playerStatusText = "Checking players...";
    
    [ObservableProperty]
    private Color playerStatusColor = Colors.Gray;
     

    [ObservableProperty]
    private bool isInitialized;
    public ObservableCollection<MarriageGameRound> Rounds { get; } = new ObservableCollection<MarriageGameRound>();

    public ObservableCollection<MarriageGame> MarriageGames { get; } = new ObservableCollection<MarriageGame>();
    public ObservableCollection<MarriageGameGroup> MarriageGamesGrouped { get; } = new ObservableCollection<MarriageGameGroup>();
  
    // Property to expose selected players for the CollectionView
    public ObservableCollection<Player> SelectedPlayers { get; private set; } = new();

    public GameSetupViewModel(IMarriageGameEngine gameEngine, IApiService apiService)
    {
        _gameEngine = gameEngine;
        _apiService = apiService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;

        try
        { 
            // Update player status
            await UpdatePlayerStatus();

            await LoadRoundsAsync();
            await LoadMarriageGameAsync();
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            // Set error states
            PlayerStatusText = "Error loading data";
        }
        finally
        {
            IsBusy = false;
        }

    }
    public async Task LoadRoundsAsync()
    {
        if (GameEngine.MarriageGameSet == null)
        {
            // Handle the case where there is no active game set
            return;
        }
        Rounds.Clear();
        var rounds = GameEngine.MarriageGameSet.Rounds;
        if (rounds == null)
        {
            await GameEngine.CreateNewGameRoundForGivenGameSet(GameEngine.MarriageGameSet!.Id);
            rounds = GameEngine.MarriageGameSet.Rounds;
        }
        if (rounds != null)
        {
            foreach (var round in rounds)
            {
                Rounds.Add(round);
            }
        }
    }

    public async Task LoadMarriageGameAsync()
    {
        MarriageGames.Clear();
        var games = GameEngine.CurrentMarriageGameRound?.MarriageGames;
        if (games is not null)
            foreach (var game in games
                .Where(g => g is not null)
                .OrderByDescending(g => g.Sequence))
            {
                MarriageGames.Add(game);
            }

        SyncMarriageGameModel(MarriageGames); 
         
    }

    private void SyncMarriageGameModel(ObservableCollection<MarriageGame> marriageGames)
    {
        GameModel.Clear();

        foreach (var marriageGame in marriageGames)
        {
            var gameModel = new MarriageRoundAndGamesModel
            {
                GameId = marriageGame.Id,
                RoundId = marriageGame.MarriageGameRoundId,
                Sequence = marriageGame.Sequence,
                Date = marriageGame.CreatedTime // You may want to add a CreatedDate property to MarriageGame
            };

            // Resolve winner from PlayerId to Player object
            if (marriageGame.WinnerId.HasValue &&
                _gameEngine.PlayerService.AllPlayers.TryGetValue(marriageGame.WinnerId.Value, out var winner))
            {
                gameModel.PlayerName = winner.Name;
            }
            else
            {
                gameModel.PlayerName = "-";
            }

            GameModel.Add(gameModel);
        }

        var grouped = GameModel.GroupBy(m => m.RoundId)
            .Select(g => new MarriageGameGroup(g.Key, g))
            .OrderByDescending(g => g.RoundId);

        MarriageGamesGrouped.Clear();
        foreach (var group in grouped)
        {
            MarriageGamesGrouped.Add(group);
        }

    }

    public ObservableCollection<MarriageRoundAndGamesModel> GameModel { get; } = new ObservableCollection<MarriageRoundAndGamesModel>();


    [RelayCommand]
    private async Task GameSettings()
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    [RelayCommand]
    private async Task ManagePlayers()
    {
        await Shell.Current.GoToAsync(nameof(PlayersPage));
    }

     [RelayCommand]
    private async Task Ready()
    { 

        IsBusy = true;

        try
        { 
            System.Diagnostics.Debug.WriteLine($"GameSetupViewModel Ready - Current game set: {_gameEngine.MarriageGameSet?.Id}");
            System.Diagnostics.Debug.WriteLine($"GameSetupViewModel Ready - Players count: {_gameEngine.MarriageGameSet?.GameSetPlayers?.Count ?? 0}");


            var canResume = await _gameEngine.ResumePreviousGameIfAvailable();
            if (!canResume)
            {
                System.Diagnostics.Debug.WriteLine("GameSetupViewModel Ready - Cannot resume, creating new game structure");

                if (_gameEngine.CurrentMarriageGameRound is null)
                {
                    if (_gameEngine.MarriageGameSet is null)
                    {
                        await _gameEngine.CreateNewGameSet();
                    }

                    // Always ensure we have a round for the game set
                    if (_gameEngine.MarriageGameSet is not null)
                    {
                        await _gameEngine.CreateNewGameRoundForGivenGameSet(_gameEngine.MarriageGameSet.Id);
                    }
                }
                else
                {
                    // We have a round but no current game, create one
                    await _gameEngine.CreateNewMarriageGame();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GameSetupViewModel Ready - Successfully resumed existing game");
            }


            // Final verification before navigation
            System.Diagnostics.Debug.WriteLine($"GameSetupViewModel Ready - Final game set: {_gameEngine.MarriageGameSet?.Id}");
            System.Diagnostics.Debug.WriteLine($"GameSetupViewModel Ready - Final round: {_gameEngine.CurrentMarriageGameRound?.Id}");
            System.Diagnostics.Debug.WriteLine($"GameSetupViewModel Ready - Final game: {_gameEngine.CurrentMarriageGame?.Id}");
            System.Diagnostics.Debug.WriteLine($"GameSetupViewModel Ready - Final scores count: {_gameEngine.CurrentMarriageGame?.MarriageGameScores?.Count ?? 0}");

            // Navigate to game page
            await Shell.Current.GoToAsync($"../{nameof(PlayGame)}");

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameSetupViewModel Ready - Error: {ex}");
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Failed to start game: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
 
    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task UpdatePlayerStatus()
    {
        try
        {
            
            var activePlayerCount = _gameEngine.MarriageGameSet?.GameSetPlayers.Count;
            
            // Update the SelectedPlayers collection for the CollectionView
            var activePlayers = _gameEngine.MarriageGameSet?.GameSetPlayers;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SelectedPlayers.Clear();
                if (activePlayers is not null)
                    foreach (var gameSetPlayer in activePlayers.Values)
                    {
                        SelectedPlayers.Add(gameSetPlayer.Player);
                    }
            });
            
            if (activePlayerCount == 0)
            {
                PlayerStatusText = "No players configured";
                PlayerStatusColor = Colors.Red;
            }
            else if (activePlayerCount == 1)
            {
                PlayerStatusText = "1 player (need at least 2)";
                PlayerStatusColor = Colors.Orange;
            }
            else
            {
                var playerNames = _gameEngine.MarriageGameSet?.GameSetPlayers.Values
                    .Take(3)
                    .Select(p => p.Player.Name)
                    .ToList();
                playerNames ??= [];

                if (activePlayerCount > 3)
                {   
                    PlayerStatusText = $"{string.Join(", ", playerNames)} and {activePlayerCount - 3} more";
                }
                else
                {
                    PlayerStatusText = string.Join(", ", playerNames);
                }
                PlayerStatusColor = Colors.Green;
            }
        }
        catch (Exception ex)
        {
            PlayerStatusText = "Error loading players: " + ex.Message;
            PlayerStatusColor = Colors.Red;
        }
    }

     

    // This method will be called when returning from settings or players pages
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            // First ensure the engine is properly initialized
            await _gameEngine.InitializeEngineAsync();
            
            // Refresh player data from the database
            await _gameEngine.RefreshPlayers();
            
            // Then update the UI
            await UpdatePlayerStatus();
           
        }
        catch (Exception ex)
        {
            PlayerStatusText = "Error refreshing data: " + ex.Message;
            PlayerStatusColor = Colors.Red;
        }
        finally
        {
            IsBusy = false;
        }
    }

     
}

public class MarriageRoundAndGamesModel
{
    public int RoundId  { get; set; }
    public int GameId { get; set; }
    public int Sequence { get; set; }
    public string? PlayerName { get; set; }
    public DateTime Date { get; set; }
    public bool Completed => !string.IsNullOrEmpty(PlayerName) && PlayerName!="-";
}

public class MarriageGameGroup : ObservableCollection<MarriageRoundAndGamesModel>
{
    public int RoundId { get; set; }
    public MarriageGameGroup(int roundId, IEnumerable<MarriageRoundAndGamesModel> games) : base(games)
    {
        RoundId = roundId;
    }
}