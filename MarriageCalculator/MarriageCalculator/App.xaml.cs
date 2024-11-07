using CommunityToolkit.Maui.Animations;

namespace MarriageCalculator;

public partial class App : Application
{
	IMarriageGameEngine MarriageGameEngine { get; }
    public App(IMarriageGameEngine marriageGameEngine)
	{
		InitializeComponent();         
		MainPage = new AppShell();
        MarriageGameEngine = marriageGameEngine;
    }
      
    protected override void OnStart()
    {
        base.OnResume();
    }
    protected override void OnSleep() {
        MarriageGameEngine.SettingsService.SaveSettingsAsync(MarriageGameEngine.CancellationTokenSource.Token);
        base.OnSleep();
    }
    public static async Task Animate(VisualElement view)
    {
        var fadeAnimation = new FadeAnimation();
        await fadeAnimation.Animate(view);
    }

}
