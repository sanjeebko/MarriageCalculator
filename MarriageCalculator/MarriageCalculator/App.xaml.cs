using CommunityToolkit.Maui.Animations;

namespace MarriageCalculator;

public partial class App : Application
{
	IMarriageGameEngine MarriageGameEngine { get; }
    public App(IMarriageGameEngine marriageGameEngine)
	{
		InitializeComponent();
        string syncLicense = @"ORg4AjUWIQA/Gnt2UlhhQlVMfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX9Sd0diWXxXdXVTT2Ve;MzU3MzcyM0AzMjM3MmUzMDJlMzBXdk5vdG15VlV6dUFUdWpmOUZpSXFBVDFOeFh6VDBHZ1lRdGZXOFJvTzNVPQ==;Mgo+DSMBMAY9C3t2UlhhQlVMfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX9Sd0diWXxXdXVSQmNc;MzU3MzcyNUAzMjM3MmUzMDJlMzBoODRrc1lDbGV5Z2taNUlaWDhVTGd2dDNPaUkxZ216R1B5S0RtOWk3TnhJPQ==;MzU3MzcyNkAzMjM3MmUzMDJlMzBXdk5vdG15VlV6dUFUdWpmOUZpSXFBVDFOeFh6VDBHZ1lRdGZXOFJvTzNVPQ==";
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncLicense);

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
