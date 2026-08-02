using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using YumeArisu.Core.Abstractions;

using WindowingMonitor = Silk.NET.Windowing.Monitor;

namespace YumeArisu.Desktop.Implements;

public sealed class DesktopWindow : IWindowControl
{
    public IView View => _window;

    private IWindow _window;
    private ScreenMode _screenMode;

    public DesktopWindow(string title, int width, int height)
    {
        GlfwWindowing.Use();

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));

        _screenMode = ScreenMode.Windowed;
        _window = Window.Create(options);
    }

    /// <summary>
    /// 전체화면과 창모드 전환을 수행합니다.
    /// </summary>
    internal void SwitchScreenMode()
    {
        if (_screenMode == ScreenMode.Windowed)
        {
            int cachedMonitorIndex = _window.Monitor.Index;

            // GLFW 안정성을 위해 최대화인 경우 경계없는 창으로 변환 후 전체화면으로
            if (_window.WindowState == WindowState.Maximized)
                ScreenMode = ScreenMode.BorderlessWindow;

            ScreenMode = ScreenMode.BorderlessFullscreen;
            
            var targetMonitor = WindowingMonitor.GetMonitors(_window)
                .FirstOrDefault(m => m.Index == cachedMonitorIndex)
                ?? WindowingMonitor.GetMainMonitor(_window);
        
            _window.Monitor = targetMonitor;   
        }
        else
        {
            ScreenMode = ScreenMode.Windowed;
        }
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