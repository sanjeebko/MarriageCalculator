using MarriageCalculator.Pages;
using MarriageCalculator.Pages.Login;
 

namespace MarriageCalculator;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(NewGame), typeof(NewGame));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage)); 
        Routing.RegisterRoute(nameof(PlayGame), typeof(PlayGame));
        Routing.RegisterRoute(nameof(PlayersPage), typeof(PlayersPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(GameSetupPage), typeof(GameSetupPage));
    }
}