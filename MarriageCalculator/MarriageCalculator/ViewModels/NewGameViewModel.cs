﻿using Android.App;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarriageCalculator.Core.Models;
using MarriageCalculator.Models;
using MarriageCalculator.Pages.NewGame;
using MarriageCalculator.Services;

namespace MarriageCalculator;

[QueryProperty(nameof(MarriageGameModel), nameof(MarriageGameModel))]
[QueryProperty(nameof(GameSettingsModel), nameof(GameSettingsModel))]
public partial class NewGameViewModel : ObservableObject
{
    private readonly IMarriageGameServices _marriageGameServices;

    [ObservableProperty]
    public MarriageGameModel marriageGameModel;

    [ObservableProperty]
    public GameSettingsModel gameSettingsModel;

    public List<KhaalModel> khaalModels;

    public NewGameViewModel(IMarriageGameServices marriageGameServices)
    {
        marriageGameModel = new();
        gameSettingsModel = new(marriageGameServices);
        khaalModels = new();
        _marriageGameServices = marriageGameServices;
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