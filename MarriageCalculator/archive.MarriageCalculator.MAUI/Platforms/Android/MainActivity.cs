using Android.App;
using Android.Content.PM;
using Android.OS;

namespace MarriageCalculator;

[Activity(Theme = "@style/Maui.SplashTheme", ScreenOrientation = ScreenOrientation.SensorLandscape| ScreenOrientation.Portrait, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Configure status bar color to match light theme gradient (will need theme detection for dynamic switching)
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && Window != null)
        {
            // For now, using a neutral color that works with both themes
            // In production, you'd want to detect the current theme and switch accordingly
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#A8B3D9"));
        }
    }
}