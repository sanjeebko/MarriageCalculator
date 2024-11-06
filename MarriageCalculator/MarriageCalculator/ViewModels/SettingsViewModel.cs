using System.Collections.ObjectModel;
using Toast = CommunityToolkit.Maui.Alerts.Toast;

namespace MarriageCalculator.ViewModels;

public partial class SettingsViewModel 
{
    public GameSettingsModel GameSettingsModel { get; set; }
    public IMarriageGameServices MarriageGameServices { get; }

    
    public ObservableCollection<Currency> Currencies { get; set; } = [Currency.GBP_Pence, Currency.USD_Cent, Currency.NPR_Rupee, Currency.INR_Rupee, Currency.EUR_Cent, Currency.AUD_Cent];
    public ObservableCollection<FoulPointBonusType> FoulPointBonuses { get; set; } = [FoulPointBonusType.NO_FOUL_POINT, FoulPointBonusType.THIS_GAME,FoulPointBonusType.NEXT_GAME];

    //Create OnCloseButtonClick event handler
    public event EventHandler? OnCloseButtonClick;



    public SettingsViewModel(IMarriageGameServices marriageGameServices    )
    {
        MarriageGameServices = marriageGameServices;
        GameSettingsModel = App.CurrentSettings.ToGameSettingsModel(marriageGameServices) as GameSettingsModel;

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
        GameSettingsService.SaveSettings(GameSettingsModel.ToGameSettings());
        App.LoadSettings();
        var toast = Toast.Make("Game Settings Saved", CommunityToolkit.Maui.Core.ToastDuration.Short);
        await toast.Show();
        await Shell.Current.GoToAsync("..");

    }
}
