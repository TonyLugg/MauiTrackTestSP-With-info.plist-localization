using Android.App;
using Android.Content.PM;
using Android.OS;
using MauiTrackTestSP.Platforms.Android;
using MauiTrackTestSP.Services;

namespace MauiTrackTestSP;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public MainActivity()
    {
        LocationTracker.MainActivityInstance = this;
        LocationTrackerService.MainActivityInstance = this;
    }
}
