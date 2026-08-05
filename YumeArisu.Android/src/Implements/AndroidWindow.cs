using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Android.Implements;

public sealed class AndroidWindow : IWindowControl
{
    public IView View { get; private set; }

    public DisplayMode _screenMode;

    public AndroidWindow()
    {
        var options = ViewOptions.Default;
        // We need to tell Silk to use OpenGLES
        // Version 3.0 is supported by >90% of devices currently in use.
        // https://developer.android.com/about/dashboards#OpenGL
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Compatability, ContextFlags.Default, new APIVersion(3, 0));
        View = Silk.NET.Windowing.Window.GetView(options); // note also GetView, instead of Window.Create.

        _screenMode = DisplayMode.Fullscreen;
    }

    public DisplayMode DisplayMode { get; set; }
    public string Title { get; set; }
    public Vector2D<int> Size { get; set; }
    public Vector2D<int> Position { get; set; }
}