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



    public MainPageViewModel()
    {
        showSettings = true;
    }
     

    public void Refresh()
    {
        IsBusy = true;
        ShowNewGame = GameEngine.IsPlayersReady;
        ShowResumeGame = GameEngine.IsActiveGame; 
        ShowSettings = !GameEngine.IsActiveGame;
        ShowPlayer = !GameEngine.IsActiveGame;
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
    public async Task InitializeAsync(IMarriageGameEngine gameEngine)
    {
        GameEngine = gameEngine;
        await GameEngine.InitializeEngineAsync();        

        Refresh();
    }



}
