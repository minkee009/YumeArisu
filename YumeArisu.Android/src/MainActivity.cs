using Android.Content.PM;
using Silk.NET.Windowing.Sdl.Android;
using YumeArisu.Android.Implements;

namespace YumeArisu.Android;

[Activity(Label = "@string/app_name", MainLauncher = true, ConfigurationChanges = ConfigChangesFlags, ScreenOrientation = ScreenOrientation.UserLandscape, Exported = true)]
public class MainActivity : SilkActivity
{
    private AndroidApplication _application;

    protected override void OnRun()
    {
        _application = new(Assets!, Context);
        _application.Run();
    }
}