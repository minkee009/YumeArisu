using System.Runtime.InteropServices;
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
    private DisplayMode _displayMode;
    private bool _isWindowsOS;

    public DesktopWindow(string title, int width, int height)
    {
        GlfwWindowing.Use();

        _isWindowsOS = OperatingSystem.IsWindows();

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
    
        _window = Window.Create(options);
    }

    /// <summary>
    /// 전체화면과 창모드 전환을 수행합니다.
    /// </summary>
    internal void SwitchDisplayMode()
    {
        if (_displayMode == DisplayMode.Windowed)
        {
            int cachedMonitorIndex = _window.Monitor.Index;

            // GLFW 안정성을 위해 최대화인 경우 경계없는 창으로 변환 후 전체화면으로
            if (_window.WindowState == WindowState.Maximized)
                DisplayMode = DisplayMode.BorderlessWindow;

            DisplayMode = DisplayMode.BorderlessFullscreen;
            
            var targetMonitor = WindowingMonitor.GetMonitors(_window)
                .FirstOrDefault(m => m.Index == cachedMonitorIndex)
                ?? WindowingMonitor.GetMainMonitor(_window);
        
            _window.Monitor = targetMonitor;   
        }
        else
        {
            DisplayMode = DisplayMode.Windowed;
        }
    }

    public DisplayMode DisplayMode
    {
        get => _displayMode;
        set
        {
            if (_displayMode == value)
                return;

            switch (value)
            {
                case DisplayMode.Windowed:
                    _window.WindowState = WindowState.Normal;
                    _window.WindowBorder = WindowBorder.Resizable;
                    break;
            
                case DisplayMode.Fullscreen:
                case DisplayMode.BorderlessFullscreen:
                    _window.WindowState = WindowState.Fullscreen;
                    _window.WindowBorder = WindowBorder.Hidden;
                    break;

                case DisplayMode.BorderlessWindow:
                    _window.WindowState = WindowState.Normal;
                    _window.WindowBorder = WindowBorder.Hidden;
                    break;
            }

            _displayMode = value;
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

    public void Present()
    {
        _window.GLContext.SwapBuffers();
        
        if(_isWindowsOS && _window.VSync && _window.WindowState != WindowState.Fullscreen)
            DwmFlush();  
    }

    [DllImport("dwmapi.dll", EntryPoint = "DwmFlush")]
    private static extern int DwmFlush();
}