using CommunityToolkit.Maui.Animations;
using CommunityToolkit.Mvvm.Messaging;
using MarriageCalculator.DataServices;
using Toast = CommunityToolkit.Maui.Alerts.Toast;

namespace MarriageCalculator.Pages;

public partial class MainPage : ContentPage
{
    public MainPageViewModel MainPageViewModel { get; }

    public MainPage( MainPageViewModel mainPageViewModel)
    {
        InitializeComponent();       
        MainPageViewModel = mainPageViewModel; 
        BindingContext = MainPageViewModel;
          
        WeakReferenceMessenger.Default.Register<NavigationReturnMessage>(this, async (sender, message) =>
        {
            await PlayAudio(message.Value);
        });
    }
    protected override async void OnAppearing()
    {
        await InitializeGameEngineAsync();
        base.OnAppearing();
    }

    #region Button Click Events     
    public static async Task ShowToast(string message)
    {
        var toast = Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Short);
        await toast.Show();
    }
     
    private async void PlayersBtn_Clicked(object sender, EventArgs e)
    {                
        await Shell.Current.GoToAsync(nameof(PlayersPage));         
    }

    #endregion

    #region Private Functions
    private async Task InitializeGameEngineAsync()
    {
        // Ensure authentication is properly initialized before initializing game engine
        await MainPageViewModel.InitializeAsync();
        
    }
    private async Task PlayAudio(string pageName)
    {
        var MarriageGameEngine = MainPageViewModel.GameEngine;
        switch (pageName)
        {
            case nameof(SettingsPage):
                await MarriageGameEngine.TextToSpeechService.SpeakAsync("मेरिज खेलको नियमहरु सुरक्षित गरियो।");
                break;
            case nameof(PlayersPage):
                var players = MarriageGameEngine.MarriageGameSet?.GameSetPlayers.Values
                    .Select(gsp => gsp.Player)
                    .Where(p => p != null && !p.Deleted)
                    .ToList();
                if (players is not null && players.Count > 0)
                {
                    await MarriageGameEngine.TextToSpeechService.SpeakAsync(players.ToArray());
                }
                break;
            case nameof(NewGame):

                break;
            default:
                break;
        }
    }

    private static async Task Animate(VisualElement view)
    {
        var fadeAnimation = new FadeAnimation();
        await fadeAnimation.Animate(view);
    }
    #endregion

}