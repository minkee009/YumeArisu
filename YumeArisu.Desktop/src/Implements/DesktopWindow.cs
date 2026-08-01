using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using YumeArisu.Core.Abstractions;

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
            _window.Monitor = GetCurrentMonitor();
            ScreenMode = ScreenMode.Fullscreen;
        }
        else
        {
            ScreenMode = ScreenMode.Windowed;
        }
    }

    internal int GetCurrentMonitorIndex() => GetCurrentMonitor()?.Index ?? -1;

    private IMonitor GetCurrentMonitor()
    {
        var windowBounds = new Rectangle<int>(_window.Position, _window.Size);
        
        IMonitor bestMonitor = null;
        int bestOverlapArea = -1;

        foreach (var monitor in Silk.NET.Windowing.Monitor.GetMonitors(_window))
        {
            Console.WriteLine($"{monitor.Name}: Origin={monitor.Bounds.Origin}, Size={monitor.Bounds.Size}");
            var monitorBounds = monitor.Bounds;

            Console.WriteLine($"Window Position={_window.Position}, Size={_window.Size}");

            // 겹치는 영역 계산
            int overlapX = Math.Max(0, 
                Math.Min(windowBounds.Origin.X + windowBounds.Size.X, monitorBounds.Origin.X + monitorBounds.Size.X) 
                - Math.Max(windowBounds.Origin.X, monitorBounds.Origin.X));
            
            int overlapY = Math.Max(0, 
                Math.Min(windowBounds.Origin.Y + windowBounds.Size.Y, monitorBounds.Origin.Y + monitorBounds.Size.Y) 
                - Math.Max(windowBounds.Origin.Y, monitorBounds.Origin.Y));

            int overlapArea = overlapX * overlapY;

            if (overlapArea > bestOverlapArea)
            {
                bestOverlapArea = overlapArea;
                bestMonitor = monitor;
            }
        }

        // 겹치는 모니터가 없으면(예외 상황) primary로 fallback
        return bestMonitor ?? Silk.NET.Windowing.Monitor.GetMainMonitor(_window);
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