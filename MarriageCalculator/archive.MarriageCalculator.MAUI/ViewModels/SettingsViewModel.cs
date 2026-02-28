using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MarriageCalculator.DataServices;
using System.Collections.ObjectModel;
using Toast = CommunityToolkit.Maui.Alerts.Toast;

namespace MarriageCalculator.ViewModels;

public partial class SettingsViewModel :ObservableObject
{
    public GameSettingsModel GameSettingsModel { get; set; }
     
    public IMarriageGameEngine MarriageGameEngine { get; }

    public ObservableCollection<Currency> Currencies { get; set; } = [Currency.GBP_Pence, Currency.USD_Cent, Currency.NPR_Rupee, Currency.INR_Rupee, Currency.EUR_Cent, Currency.AUD_Cent];
    public ObservableCollection<FoulPointBonusType> FoulPointBonuses { get; set; } = [FoulPointBonusType.NO_FOUL_POINT, FoulPointBonusType.THIS_GAME,FoulPointBonusType.NEXT_GAME];

    
    public event EventHandler? OnCloseButtonClick;



    public SettingsViewModel(IMarriageGameEngine marriageGameEngine  )
    {
        MarriageGameEngine = marriageGameEngine;
        GameSettingsModel = marriageGameEngine.SettingsService.Settings!.ToGameSettingsModel( ) as GameSettingsModel;
    }
    [RelayCommand]
    public static async Task BackButtonClick()
    {
        var toast = Toast.Make("Changes have been discarded.", CommunityToolkit.Maui.Core.ToastDuration.Short);
        
        await toast.Show(); 
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task SaveSettingsClick() {
        MarriageGameEngine.SettingsService.Settings = GameSettingsModel.ToGameSettings();
        
        await MarriageGameEngine.SettingsService.SaveSettingsAsync();
        await MarriageGameEngine.SettingsService.LoadSettingsAsync();
        MarriageGameEngine.TextToSpeechService.Mute = !MarriageGameEngine.SettingsService.Settings.Audio;

        WeakReferenceMessenger.Default.Send(new NavigationReturnMessage(nameof(SettingsPage)));
        var toast = Toast.Make("Game Settings Saved", CommunityToolkit.Maui.Core.ToastDuration.Short);
        await toast.Show();
        await Shell.Current.GoToAsync("..");

    }
}
