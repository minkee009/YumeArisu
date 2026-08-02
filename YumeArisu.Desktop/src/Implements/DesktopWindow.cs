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
            bool isMaximumized = _window.WindowState == WindowState.Maximized;
            ScreenMode = ScreenMode.BorderlessFullscreen;
            
            // TODO : GLFW 버그가 있음 -> 최대화 상태에서 전체화면 후 창모드로 복귀 시 Resizble이 적용되지 않아 화면 타이틀 바를 잃어버림... (한 번 더 전체화면 후 창모드로 복귀 시 정상 작동)
            // 추가로 모니터 2대 이상 시 최대화가 보조 모니터에 걸려있던 상황임에도 전체화면 전환 시 주 모니터로 이동함
            if(!isMaximumized)
            {
                var targetMonitor = WindowingMonitor.GetMonitors(_window)
                    .FirstOrDefault(m => m.Index == cachedMonitorIndex)
                    ?? WindowingMonitor.GetMainMonitor(_window);
            
                _window.Monitor = targetMonitor;   
            }
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
                    _window.TopMost = false;
                    _window.WindowState = WindowState.Normal;
                    _window.WindowBorder = WindowBorder.Resizable;
                    break;
            
                case ScreenMode.Fullscreen:
                case ScreenMode.BorderlessFullscreen:
                    _window.TopMost = false;
                    _window.WindowState = WindowState.Fullscreen;
                    _window.WindowBorder = WindowBorder.Hidden;
                    break;

                case ScreenMode.BorderlessWindow:
                    _window.TopMost = false;
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