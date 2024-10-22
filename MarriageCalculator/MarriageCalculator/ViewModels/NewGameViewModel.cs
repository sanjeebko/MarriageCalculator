using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarriageCalculator.Models;
using MarriageCalculator.Pages;
 
using MarriageCalculator.Services;

namespace MarriageCalculator;

[QueryProperty(nameof(MarriageGameModel), nameof(MarriageGameModel))]
[QueryProperty(nameof(GameSettingsModel), nameof(GameSettingsModel))]
public partial class NewGameViewModel : ObservableObject
{
    private readonly IMarriageGameServices _marriageGameServices;
    public PlayerSettingsViewModel GameSettingsService { get; }

    [ObservableProperty]
    public MarriageGameModel marriageGameModel;

    [ObservableProperty]
    public GameSettingsModel gameSettingsModel;

    public List<KhaalModel> khaalModels;

    public NewGameViewModel(IMarriageGameServices marriageGameServices, PlayerSettingsViewModel gameSettingsService)
    {
        marriageGameModel = new();
        gameSettingsModel = new(marriageGameServices);
        khaalModels = new();
        _marriageGameServices = marriageGameServices;
        GameSettingsService = gameSettingsService;
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
        marriageGameModel = new();
        //OnPropertyChanged(nameof(MarriageGameModel));
        gameSettingsModel = new(_marriageGameServices);
        // OnPropertyChanged(nameof(gameSettingsModel));
    }
}