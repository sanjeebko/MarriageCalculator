using MarriageCalculator.Pages;
 

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
    }
}