using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Game.Scenes;

namespace YumeArisu.Desktop.Implements;

public class DesktopApplication : IApplication
{
    private DesktopWindow _window;
    private DesktopFileIO _fileIO;

    public DesktopApplication(string title, int width, int height)
    {
        _window = new(title, width, height);
        _window.View.Load += OnLoad;
        _window.View.Resize += OnResized;
        _window.View.Update += OnUpdate;
        _window.View.Render += OnRender;
        _window.View.Closing += OnClosing;

        _fileIO = new();
    }

    public void Run()
    {
        _window.View.Run();
    }

    public void OnLoad()
    {
        ApplicationSystem.Instance.StartUp(this);
        WindowSystem.Instance.StartUp(_window);
        TimeSystem.Instance.StartUp(default);
        InputSystem.Instance.StartUp(_window.View);
        SceneSystem.Instance.StartUp([new TestScene1(), new TestScene2(), new StaticScene()]); // 구현이 지저분한데 좀 더 이쁘고 관리하기 편하게 만들 순 없을까 고민해보기
    }

    public void OnClosing()
    {
        SceneSystem.Instance.ShutDown();
        TimeSystem.Instance.ShutDown();
        InputSystem.Instance.ShutDown();
        WindowSystem.Instance.ShutDown();
        ApplicationSystem.Instance.ShutDown();
    }

    public void OnResized(Vector2D<int> newSize)
    {
        // Handle view resizing if necessary
    }

    public void OnUpdate(double deltaTime)
    {
        SceneSystem.Instance.Update();
        TimeSystem.Instance.BeginFrame(deltaTime);
        TimeSystem.Instance.ConsumeFixedSteps(
            _ => 
            {
                // BehaviourSystem.Instance.FixedUpdate(_);
                // CoroutineSystem.Instance.FixedUpdate();
            }
        );
    }

    public void OnRender(double deltaTime)
    {
        
    }
    
    public bool IsRunning()
    {
        return !_window.View.IsClosing;
    }

    public void RequestClose()
    {
        _window.View.Close();
    }


    public void SetTargetFrameRate(int fps)
    {
        _window.View.FramesPerSecond = Math.Max(0, fps);
    }

    public void SetVSync(bool enabled)
    {
        _window.View.VSync = enabled;
    }
}