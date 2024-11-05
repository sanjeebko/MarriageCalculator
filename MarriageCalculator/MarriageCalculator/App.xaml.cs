using MarriageCalculator.Core.Models;
using MarriageCalculator.Services;

namespace MarriageCalculator;

public partial class App : Application
{
	public static GameSettings CurrentSettings { get; set; }
    public App()
	{
		InitializeComponent();

		CurrentSettings = GameSettingsService.LoadSettings();

		MainPage = new AppShell();
	}
     
    protected override void OnStart()
    {
        CurrentSettings = GameSettingsService.LoadSettings();
        base.OnResume();
    }
    protected override void OnSleep() {
        GameSettingsService.SaveSettings(CurrentSettings);
        base.OnSleep();
    }


}
