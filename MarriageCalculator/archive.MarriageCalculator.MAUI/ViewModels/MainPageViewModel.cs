using CommunityToolkit.Mvvm.ComponentModel;

namespace MarriageCalculator.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public IMarriageGameEngine GameEngine { get; set; }
    
    [ObservableProperty]
    public bool isBusy;
    
    [ObservableProperty]
    public bool showResumeGame;
    
    [ObservableProperty]
    public bool showNewGame;

    [ObservableProperty]
    public bool showSettings;
      
    [ObservableProperty]
    public bool showPlayer;

    [ObservableProperty]
    public bool isServerConnected;

    [ObservableProperty]
    public bool isConnecting;

    public MainPageViewModel()
    {
        showSettings = true;
        isServerConnected = false;
        isConnecting = true; // Start in connecting state
    }
     

    public void Refresh()
    {
        IsBusy = true;
        ShowNewGame = GameEngine.IsPlayersReady && IsServerConnected;
        ShowResumeGame = GameEngine.IsActiveGame && IsServerConnected; 
        ShowSettings = !GameEngine.IsActiveGame && IsServerConnected;
        ShowPlayer = !GameEngine.IsActiveGame && IsServerConnected;
        IsBusy = false;
    }

    [RelayCommand]     
    async Task NewGameAsync()
    {
        await GameEngine.CloseCurrentGameSet();
        await GameEngine.CreateNewGameSet();        
        await Shell.Current.GoToAsync(nameof(PlayGame));
    }

    [RelayCommand]
    public async Task ResumeGame()
    {
        if(GameEngine.MarriageGameSet is null)
        {
            Refresh();
            return;
        }
        var canResume =await GameEngine.ResumePreviousGameIfAvailable();
        if (!canResume)
        {
            if (GameEngine.CurrentMarriageGameRound is null)
            {
                if (GameEngine.MarriageGameSet is null)
                {
                   await GameEngine.CreateNewGameSet();
                }
                else
                {
                    await GameEngine.CreateNewGameRoundForGivenGameSet(GameEngine.MarriageGameSet.Id);
                }
            }else
                await GameEngine.CreateNewMarriageGameForGivenGameRound(GameEngine.CurrentMarriageGameRound);

        }
        await Shell.Current.GoToAsync(nameof(PlayGame));
        
    }
    
    [RelayCommand]
    public async Task ResetGame()
    {
       await GameEngine.CleanMarriageGameSet();
       await GameEngine.InitializeEngineAsync();
       Refresh();
    }

    [RelayCommand] 
    public async Task  GameSettingsPage() {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    [RelayCommand]
    public async Task  PlayerSettingsPage()
    {
        await Shell.Current.GoToAsync(nameof(PlayersPage));
    }

    [RelayCommand]
    public void Exit()
    {
        Application.Current.Quit();
    }

    [RelayCommand]
    public async Task RetryConnection()
    {
        IsBusy = true;
        IsConnecting = true;
        IsServerConnected = false;
        
        try
        {
            // Test the server connection using the DbService
            IsServerConnected = await GameEngine.DatabaseService.TestConnectionAsync();
            
            if (IsServerConnected)
            {
                // Re-initialize the game engine if connection is successful
                await GameEngine.InitializeEngineAsync();
            }
        }
        catch (Exception)
        {
            IsServerConnected = false;
        }
        finally
        {
            IsConnecting = false;
            IsBusy = false;
            Refresh();
        }
    }

    public async Task InitializeAsync(IMarriageGameEngine gameEngine)
    {
        GameEngine = gameEngine;
        IsConnecting = true;
        IsServerConnected = false;
        
        try
        {
            // Test connection first
            IsServerConnected = await GameEngine.DatabaseService.TestConnectionAsync();
            
            if (IsServerConnected)
            {
                await GameEngine.InitializeEngineAsync();
            }
        }
        catch (Exception)
        {
            IsServerConnected = false;
        }
        finally
        {
            IsConnecting = false;
        }

        Refresh();
    }
}
