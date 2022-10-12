using Toast = CommunityToolkit.Maui.Alerts.Toast;
using CommunityToolkit.Maui.Animations;

namespace MarriageCalculator;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        SemanticScreenReader.Announce("Closing...");

        Application.Current?.Quit();
    }

    private async void StartBtn_Clicked(object sender, EventArgs e)
    {
        var toast = Toast.Make("Start a new game", CommunityToolkit.Maui.Core.ToastDuration.Short);

        await toast.Show();
        var fadeAnimation = new FadeAnimation();
        await fadeAnimation.Animate(StartBtn);

        await Navigation.PushAsync(new Pages.NewGame.NewGame());
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
}