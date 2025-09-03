using Toast = CommunityToolkit.Maui.Alerts.Toast;
using CommunityToolkit.Maui.Animations;
using CommunityToolkit.Mvvm.Messaging;
using MarriageCalculator.DataServices;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Pages;

public partial class MainPage : ContentPage
{
    public IMarriageGameEngine MarriageGameEngine { get; }
    public MainPageViewModel MainPageViewModel { get; }

    public MainPage(IMarriageGameEngine marriageGameEngine, MainPageViewModel mainPageViewModel)
    {
        InitializeComponent();
        MarriageGameEngine = marriageGameEngine;
        MainPageViewModel = mainPageViewModel;
        MarriageGameEngine.LastPageName = nameof(MainPage);
         
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
        
        if (!MarriageGameEngine.Initialized)
        {
            await MarriageGameEngine.InitializeEngineAsync();
        }
    }
    private async Task PlayAudio(string pageName)
    {
        switch (pageName)
        {
            case nameof(SettingsPage):
                await MarriageGameEngine.TextToSpeechService.SpeakAsync("मेरिज खेलको नियमहरु सुरक्षित गरियो।");
                break;
            case nameof(PlayersPage): 
                await MarriageGameEngine.TextToSpeechService.SpeakAsync([.. MarriageGameEngine.PlayerService.ActivePlayers.Values]);
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