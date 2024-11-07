using Toast = CommunityToolkit.Maui.Alerts.Toast;
using CommunityToolkit.Maui.Animations;
using CommunityToolkit.Mvvm.Messaging;
using MarriageCalculator.DataServices;

namespace MarriageCalculator.Pages;

public partial class MainPage : ContentPage
{
    public IMarriageGameEngine MarriageGameEngine { get; }

    public MainPage(IMarriageGameEngine marriageGameEngine)
    {
        InitializeComponent();
        MarriageGameEngine = marriageGameEngine;
         
        MarriageGameEngine.LastPageName = nameof(MainPage);
        _ = InitializeGameEngineAsync();

        WeakReferenceMessenger.Default.Register<NavigationReturnMessage>(this, async (sender, message) =>
        {
            await PlayAudio(message.Value);
        });
    }
    private async Task PlayAudio(string pageName)
    {
         switch(pageName)
        {
            case nameof(SettingsPage):
                await MarriageGameEngine.TextToSpeechService.SpeakAsync("मेरिज खेलको नियमहरु सुरक्षित गरियो।");
                break;
            case nameof(PlayersPage):
                await MarriageGameEngine.TextToSpeechService.SpeakAsync(MarriageGameEngine.PlayerService.Players.ToArray());
                break;
            case nameof(NewGame):
                 
                break;
            default:
                break;

        }
         
    }

    private async Task InitializeGameEngineAsync()
    {
        if (!MarriageGameEngine.Initialized) { 
            await MarriageGameEngine.InitializeEngineAsync();
        }
         
    }
    
    private void OnCloseClicked(object sender, EventArgs e)
    {
        SemanticScreenReader.Announce("Closing...");

        Application.Current?.Quit();
    }

    private async void StartBtn_Clicked(object sender, EventArgs e)
    {
        await Animate(StartBtn);
                
        await Shell.Current.GoToAsync(nameof(NewGame));
    }

    private static async Task Animate(VisualElement view)
    {
        var fadeAnimation = new FadeAnimation();
        await fadeAnimation.Animate(view);
    }

    private async void ResumeBtn_Clicked(object sender, EventArgs e)
    {
        var toast = Toast.Make("Resume previous game", CommunityToolkit.Maui.Core.ToastDuration.Short);
        await toast.Show();
        var fadeAnimation = new FadeAnimation();
        await fadeAnimation.Animate(ResumeBtn);
        Thread.Sleep(1000);
        await fadeAnimation.Animate(ResumeBtn);
    }

    public static async Task ShowToast(string message)
    {
        var toast = Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Short);
        await toast.Show();
    }

    private async void SettingsBtn_Clicked(object sender, EventArgs e)
    {
        await Animate(SettingsBtn);  
        await Shell.Current.GoToAsync(nameof(SettingsPage));
                
    }

    private async void PlayersBtn_Clicked(object sender, EventArgs e)
    {
        await Animate(PlayersBtn);        
        await Shell.Current.GoToAsync(nameof(PlayersPage));
         
    }
     
}