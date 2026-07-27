using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;
using Silk.NET.Input.Sdl;
using YumeArisu.Core.Abstractions;


namespace YumeArisu.Desktop.Implements;

public sealed class DesktopWindow : IWindowControl
{
    public IView View => _window;
    private IWindow _window;
    private ScreenMode _screenMode;

    public DesktopWindow(string title, int width, int height)
    {
        SdlWindowing.RegisterPlatform();
        SdlInput.RegisterPlatform();

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;

        _screenMode = ScreenMode.Windowed;

        _window = Window.Create(options);
    }

    public ScreenMode ScreenMode
    {
        get => _screenMode;
        set
        {
            if (_screenMode == value)
                return;

            switch (value)
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

            _screenMode = value;
        }
    }

    public string Title 
    { 
        get => _window.Title; 
        set => _window.Title = value; 
    }

    public Vector2D<int> Size 
    { 
        get => _window.Size; 
        set => _window.Size = value; 
    }

    public Vector2D<int> Position 
    { 
        get => _window.Position; 
        set => _window.Position = value; 
    }
}