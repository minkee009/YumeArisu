using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Abstractions;
using YumeArisu.Game.Scenes.Manifset;

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
        BehaviourSystem.Instance.StartUp(default);
        SceneSystem.Instance.StartUp(new TestSceneManifest());
    }

    public void OnClosing()
    {
        SceneSystem.Instance.ShutDown();
        BehaviourSystem.Instance.ShutDown();
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
        SceneSystem.Instance.BeginFrame();
        BehaviourSystem.Instance.BeginFrame();
        TimeSystem.Instance.BeginFrame(deltaTime);
        BehaviourSystem.Instance.ExecuteAwake();
        BehaviourSystem.Instance.ExecuteOnEnable();
        BehaviourSystem.Instance.ExecuteStart();
        TimeSystem.Instance.ConsumeFixedSteps(
            (_) => 
            {
                BehaviourSystem.Instance.ExecuteFixedUpdate();
                // CoroutineSystem.Instance.FixedUpdate();
            }
        );
        BehaviourSystem.Instance.ExecuteUpdate();
        BehaviourSystem.Instance.ExecuteLateUpdate();
        BehaviourSystem.Instance.ExecuteOnDisable();
        BehaviourSystem.Instance.ExecuteOnDestroy();
        InputSystem.Instance.EndFrame();
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