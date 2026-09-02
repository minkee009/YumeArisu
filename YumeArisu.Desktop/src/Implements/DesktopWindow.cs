using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using StbImageSharp;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Desktop.Implements;

using WindowingMonitor = Silk.NET.Windowing.Monitor;

public sealed class DesktopWindow : IWindowControl
{
    public IView View => _window;

    private IWindow _window;
    private DisplayMode _displayMode;
    private DisplayMode _trueDisplayMode;
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

        if(OperatingSystem.IsLinux())
        {
            _window.Load += 
                () =>
                {
                    unsafe
                    {
                        GlfwCallbacks.WindowRefreshCallback noOpRefreshCallback = _ => { };
                        Glfw.GetApi().SetWindowRefreshCallback((WindowHandle*)_window.Handle, noOpRefreshCallback);
                    }
                };
        }
    }

    public void SetWindowIcon(byte[] iconData, int width, int height)
    {
        StbImage.stbi_set_flip_vertically_on_load(0);
        ImageResult image = ImageResult.FromMemory(iconData, ColorComponents.RedGreenBlueAlpha);

        var rawImage = new RawImage(width, height, new Memory<byte>(image.Data));
        _window.SetWindowIcon(new[] { rawImage });
    }

    /// <summary>
    /// 전체화면과 창모드 전환을 수행합니다.
    /// </summary>
    internal void SwitchDisplayMode()
    {
        if (_displayMode == DisplayMode.Windowed || _displayMode == DisplayMode.BorderlessWindow)
        {
            DisplayMode = DisplayMode.BorderlessFullscreen;
        }
        else
        {
            DisplayMode = DisplayMode.Windowed;
        }
    }

    public void ApplyDisplayMode()
    {
        if(_displayMode == _trueDisplayMode)
            return;

        var isFullScreenMode = _displayMode == DisplayMode.Fullscreen || _displayMode == DisplayMode.BorderlessFullscreen;
        var wasWindowedMode =  _trueDisplayMode == DisplayMode.Windowed || _trueDisplayMode == DisplayMode.BorderlessWindow;

        int cachedMonitorIndex = -1;

        // GLFW 안정성을 위해 최대화인 경우 경계없는 창으로 변환 후 전체화면으로
        if(isFullScreenMode && wasWindowedMode)
        {
            cachedMonitorIndex = _window.Monitor.Index;
  
            if (_window.WindowState == WindowState.Maximized)
            {
                _window.WindowState = WindowState.Normal;
                _window.WindowBorder = WindowBorder.Hidden;
            }
        }

        switch (_displayMode)
        {
            case DisplayMode.Windowed:
                _window.WindowState = WindowState.Normal;
                _window.WindowBorder = WindowBorder.Resizable;
                break;
        
            case DisplayMode.Fullscreen:
            case DisplayMode.BorderlessFullscreen:
                _window.WindowState = WindowState.Fullscreen;
                _window.WindowBorder = WindowBorder.Hidden;

                if(cachedMonitorIndex != -1)
                {
                    var targetMonitor = WindowingMonitor.GetMonitors(_window)
                    .FirstOrDefault(m => m.Index == cachedMonitorIndex)
                    ?? WindowingMonitor.GetMainMonitor(_window);
        
                    _window.Monitor = targetMonitor; 
                }
                break;

            case DisplayMode.BorderlessWindow:
                _window.WindowState = WindowState.Normal;
                _window.WindowBorder = WindowBorder.Hidden;
                break;
        }

        _trueDisplayMode = _displayMode;
    }

    public DisplayMode DisplayMode
    {
        get => _displayMode;
        set => _displayMode = value;
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