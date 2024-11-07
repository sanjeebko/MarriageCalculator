using CommunityToolkit.Mvvm.ComponentModel;

namespace MarriageCalculator.ViewModels;

[QueryProperty(nameof(MarriageGameModel), nameof(MarriageGameModel))]
[QueryProperty(nameof(GameSettingsModel), nameof(GameSettingsModel))]
public partial class NewGameViewModel : ObservableObject
{
    public PlayerSettingsViewModel PlayerSettingsService { get; }
     

    [ObservableProperty]
    public MarriageGameModel marriageGameModel;

    [ObservableProperty]
    public GameSettingsModel gameSettingsModel;

     

    public NewGameViewModel( PlayerSettingsViewModel gameSettingsService)
    {
        marriageGameModel = new();
        gameSettingsModel = new();
         
        PlayerSettingsService = gameSettingsService;
        
    }
        

    [RelayCommand]
    public async Task BackButtonClick()
    => await Shell.Current.GoToAsync("..", true, new Dictionary<string, object>
    {
        [nameof(MarriageGameModel)] = MarriageGameModel,
        [nameof(GameSettingsModel)] = GameSettingsModel
    });

    [RelayCommand]
    public async Task StartGameClick()
    {
        await Shell.Current.GoToAsync(nameof(PlayGame), true, new Dictionary<string, object>
        {
            [nameof(MarriageGameModel)] = MarriageGameModel,
            [nameof(GameSettingsModel)] = GameSettingsModel
        });
    }

    internal void Reset()
    {
        MarriageGameModel = new();
        //OnPropertyChanged(nameof(MarriageGameModel));
        GameSettingsModel = new();
        // OnPropertyChanged(nameof(gameSettingsModel));
    }

     
}