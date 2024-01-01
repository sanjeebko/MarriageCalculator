using Toast = CommunityToolkit.Maui.Alerts.Toast;
using CommunityToolkit.Maui.Animations;
using MarriageCalculator.Pages.NewGame;

namespace MarriageCalculator;

public partial class MainPage : ContentPage
{
    private readonly NewGameViewModel _newGameViewModel;

    public MainPage(NewGameViewModel newGameViewModel)
    {
        InitializeComponent();
        _newGameViewModel = newGameViewModel;
        BindingContext = _newGameViewModel;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        SemanticScreenReader.Announce("Closing...");

        Application.Current?.Quit();
    }

    private async void StartBtn_Clicked(object sender, EventArgs e)
    {
        await MainPage.Animate(StartBtn);

        _newGameViewModel.Reset();
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
}