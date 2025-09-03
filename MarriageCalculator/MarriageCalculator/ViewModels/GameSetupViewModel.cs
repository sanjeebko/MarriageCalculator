using CommunityToolkit.Mvvm.ComponentModel;
using MarriageCalculator.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MarriageCalculator.ViewModels;

public partial class GameSetupViewModel : ObservableObject
{
    private readonly IMarriageGameEngine _gameEngine;
    private readonly IApiService _apiService;
    
    [ObservableProperty]
    private bool isBusy;
    
    [ObservableProperty]
    private bool isNewGame;
    
    [ObservableProperty]
    private string playerStatusText = "Checking players...";
    
    [ObservableProperty]
    private Color playerStatusColor = Colors.Gray;
    
    [ObservableProperty]
    private bool canStartGame;

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
            // Get navigation parameters
            var parameters = Shell.Current.CurrentState.Location.ToString();
            
            IsNewGame = parameters.Contains("newgame=true");
            
            // Update player status
            await UpdatePlayerStatus();
            
            // Check if we can start the game
            UpdateCanStartGame();
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
        if (!CanStartGame)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Cannot Start Game",
                "Please ensure you have at least 2 players configured.",
                "OK");
            return;
        }

        IsBusy = true;
        
        try
        {
            if (IsNewGame)
            {
                // Set up new game
                await _gameEngine.CloseCurrentGameSet();
                await _gameEngine.CreateNewGameSet();
            }
            else
            {
                // Resume existing game
                var canResume = await _gameEngine.ResumePreviousGameIfAvailable();
                if (!canResume)
                {
                    if (_gameEngine.CurrentMarriageGameRound is null)
                    {
                        if (_gameEngine.MarriageGameSet is null)
                        {
                            await _gameEngine.CreateNewGameSet();
                        }
                        else
                        {
                            await _gameEngine.CreateNewGameRoundForGivenGameSet(_gameEngine.MarriageGameSet.Id);
                        }
                    }
                    else
                    {
                        await _gameEngine.CreateNewMarriageGameForGivenGameRound(_gameEngine.CurrentMarriageGameRound);
                    }
                }
            }
            
            // Navigate to game page
            await Shell.Current.GoToAsync(nameof(PlayGame));
        }
        catch (Exception ex)
        {
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
            var activePlayerCount = _gameEngine.PlayerService.ActivePlayers.Count;
            
            // Update the SelectedPlayers collection for the CollectionView
            var activePlayers = _gameEngine.PlayerService.ActivePlayers.Values.ToList();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SelectedPlayers.Clear();
                foreach (var player in activePlayers)
                {
                    SelectedPlayers.Add(player);
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
                var playerNames = _gameEngine.PlayerService.ActivePlayers.Values
                    .Take(3)
                    .Select(p => p.Name)
                    .ToList();
                
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

    private void UpdateCanStartGame()
    {
        CanStartGame = _gameEngine.PlayerService.ActivePlayers.Count >= 2 && 
                       _gameEngine.Initialized;
    }

    // This method will be called when returning from settings or players pages
    public async Task RefreshAsync()
    {
        await UpdatePlayerStatus();
        UpdateCanStartGame();
    }
}