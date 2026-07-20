using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Desktop.Implements;

public class DesktopWindow : IWindowControl
{
    public IView View => _window;
    private IWindow _window;
    private ScreenMode _screenMode;

    public DesktopWindow(string title, int width, int height)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;

        _window = Window.Create(options);
        _screenMode = ScreenMode.Windowed;
    }

    public void SetScreenMode(ScreenMode screenMode)
    {
        if(_screenMode == screenMode)
            return;

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

        _screenMode = screenMode;
    }
    public void SetTitle(string title) => _window.Title = title;
    public void SetSize(int width, int height) => _window.Size = new(width,height);
    public void SetPosition(int x, int y) => _window.Position = new(x,y);

    public ScreenMode GetScreenMode() => _screenMode;
    public string GetTitle() => _window.Title;
    public Vector2D<int> GetSize() => _window.Size;
    public Vector2D<int> GetPosition() => _window.Position;
}