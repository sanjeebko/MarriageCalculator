using MarriageCalculator.Pages.NewGame;

namespace MarriageCalculator;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(NewGame), typeof(NewGame));
        Routing.RegisterRoute(nameof(NewGamePlayers), typeof(NewGamePlayers));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(PlayGame), typeof(PlayGame));
    }
}