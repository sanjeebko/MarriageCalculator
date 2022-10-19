using MarriageCalculator.Pages.NewGame;

namespace MarriageCalculator;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(NewGame), typeof(NewGame));
    }
}