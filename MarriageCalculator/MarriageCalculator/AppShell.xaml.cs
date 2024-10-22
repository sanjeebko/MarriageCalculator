using MarriageCalculator.Pages;
 

namespace MarriageCalculator;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(NewGame), typeof(NewGame));
        Routing.RegisterRoute(nameof(Settings), typeof(Settings));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(PlayGame), typeof(PlayGame));
        Routing.RegisterRoute(nameof(Players), typeof(Players));
    }
}