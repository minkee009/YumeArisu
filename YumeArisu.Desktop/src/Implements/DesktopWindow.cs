using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Desktop.Implements;

public class DesktopWindow : IWindowControl
{
    public IView View => _window;
    private IWindow _window;

    public DesktopWindow(string title, int width, int height)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;

        _window = Window.Create(options);
    }

    public void SetScreenMode(ScreenMode screenMode)
    {
        switch (screenMode)
        {
            case ScreenMode.Windowed:
                _window.WindowState = WindowState.Normal;
                _window.WindowBorder = WindowBorder.Resizable;
                break;

            case ScreenMode.Fullscreen:
            case ScreenMode.BorderlessFullscreen:
                _window.WindowState = WindowState.Fullscreen;
                _window.WindowBorder = WindowBorder.Hidden;
                break;

            case ScreenMode.BorderlessWindow:
                _window.WindowState = WindowState.Normal;
                _window.WindowBorder = WindowBorder.Hidden;
                break;
        }
    }

    public void SetTitle(string title)
    {
        _window.Title = title;
    }

    public void SetSize(int width, int height)
    {
        _window.Size = new (width,height);
    }

    public void SetPosition(int x, int y)
    {
        _window.Position = new (x,y);
    }
}