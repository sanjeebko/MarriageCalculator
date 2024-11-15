using Android.App;
using Android.Content.PM;

namespace MarriageCalculator;

[Activity(Theme = "@style/Maui.SplashTheme", ScreenOrientation = ScreenOrientation.SensorLandscape| ScreenOrientation.Portrait, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}