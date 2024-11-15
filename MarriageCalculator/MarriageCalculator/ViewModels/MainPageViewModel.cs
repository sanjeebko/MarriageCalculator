using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public void Initialize(IMarriageGameEngine gameEngine)
    {
       GameEngine = gameEngine;
        Refresh();
    }

    public void Refresh()
    {
        IsBusy = true;        
        ShowResumeGame = GameEngine.IsActiveGame;
        ShowNewGame = !GameEngine.IsActiveGame && GameEngine.IsPlayersReady;
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
        await GameEngine.DbServices.CleanMarriageGameSet();
        await GameEngine.CloseCurrentGameSet();
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



}
