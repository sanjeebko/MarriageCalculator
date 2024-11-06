using CommunityToolkit.Maui.Animations;
using MarriageCalculator.Core.Models;
using MarriageCalculator.Services;

namespace MarriageCalculator;

public partial class App : Application
{
	public static GameSettings CurrentSettings { get; set; }
    public App()
	{
		InitializeComponent();

        LoadSettings();

		MainPage = new AppShell();
	}
     public static void  LoadSettings()
    {
        CurrentSettings = GameSettingsService.LoadSettings();
    }
    protected override void OnStart()
    {
        LoadSettings();
        
        base.OnResume();
    }
    protected override void OnSleep() {
        GameSettingsService.SaveSettings(CurrentSettings);
        base.OnSleep();
    }
    public static async Task Animate(VisualElement view)
    {
        var fadeAnimation = new FadeAnimation();
        await fadeAnimation.Animate(view);
    }

}
