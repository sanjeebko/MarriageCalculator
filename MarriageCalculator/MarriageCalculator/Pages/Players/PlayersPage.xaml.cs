using CommunityToolkit.Mvvm.Messaging;
using MarriageCalculator.DataServices;

namespace MarriageCalculator.Pages;

public partial class PlayersPage : ContentPage
{
    public PlayersPage(PlayerSettingsViewModel viewModel)
    {
        InitializeComponent();
        PlayerSettingsViewModel = viewModel;
        
        BindingContext = PlayerSettingsViewModel; 
        PlayerSettingsViewModel.OnComplete += PlayerSettingsViewModel_OnComplete;
        PlayerSettingsViewModel.OnError += PlayerSettingsViewModel_OnError;
       
    } 

    private void PlayerSettingsViewModel_OnError(object? sender, EventArgs e)
    {
        
    }

    private async void PlayerSettingsViewModel_OnComplete(object? sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new NavigationReturnMessage(nameof(PlayersPage)));
        await  Shell.Current.GoToAsync("..");
    }

    public PlayerSettingsViewModel PlayerSettingsViewModel { get; }
   

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Debug logging to understand the issue
        System.Diagnostics.Debug.WriteLine($"PlayersPage OnAppearing - MarriageGameSet: {PlayerSettingsViewModel.MarriageGameEngine.MarriageGameSet?.Id}, IsActive: {PlayerSettingsViewModel.MarriageGameEngine.MarriageGameSet?.IsActive}");
        
        // Ensure players are loaded when the page appears
        try
        {
            // First ensure the engine is initialized to prevent creating new game sets
            await PlayerSettingsViewModel.MarriageGameEngine.InitializeEngineAsync();
            
            await PlayerSettingsViewModel.RefreshAllPlayersAsync();
            await PlayerSettingsViewModel.RefreshCurrentPlayerAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PlayersPage OnAppearing error: {ex}");
        }
    }

    
    //private static async Task SpeakTextInNepali(string text)
    //{
    //    // Get available locales
    //    var locales = await TextToSpeech.Default.GetLocalesAsync();
    //    // Find the Nepali locale
    //    var nepaliLocale = locales.FirstOrDefault(locale => locale.Language == "ne" && locale.Country == "NP");
    //    if (nepaliLocale != null)
    //    {
    //        var options = new SpeechOptions
    //        {
    //            Locale = nepaliLocale,
    //            Pitch = 1.0f,
    //            Volume = 1.0f
    //        };
            
    //        // Speak the text in Nepali
    //        await TextToSpeech.Default.SpeakAsync(text, options);
    //    }
    //    else
    //    {
    //        // Fallback if Nepali locale is not found
    //        await TextToSpeech.Default.SpeakAsync("Nepali locale not found.");
    //    }
    //}
}