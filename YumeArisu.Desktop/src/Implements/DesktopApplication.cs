using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Abstractions;
using YumeArisu.Game.Scenes.Manifset;

namespace YumeArisu.Desktop.Implements;

public sealed class DesktopApplication : IApplicationControl
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
        ResourceSystem.Instance.StartUp(_fileIO);
        BehaviourSystem.Instance.StartUp(default);
        CoroutineSystem.Instance.StartUp(default);
        SceneSystem.Instance.StartUp(new TestSceneManifest());

        SceneSystem.Instance.OnBeforeSceneChange += CoroutineSystem.Instance.ImmediateStopAllCoroutines;
    }

    public void OnClosing()
    {
        SceneSystem.Instance.ShutDown();
        CoroutineSystem.Instance.ShutDown();
        BehaviourSystem.Instance.ShutDown();
        TimeSystem.Instance.ShutDown();
        ResourceSystem.Instance.ShutDown();
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
        TimeSystem.Instance.BeginFrame(deltaTime);
        SceneSystem.Instance.BeginFrame();
        BehaviourSystem.Instance.BeginFrame();
        BehaviourSystem.Instance.ExecuteAwake();
        BehaviourSystem.Instance.ExecuteOnEnable();
        BehaviourSystem.Instance.ExecuteStart();
        TimeSystem.Instance.ConsumeFixedSteps(
            (_) => 
            {
                BehaviourSystem.Instance.ExecuteFixedUpdate();
                CoroutineSystem.Instance.YieldFixedUpdate();
            }
        );
        BehaviourSystem.Instance.ExecuteUpdate();
        BehaviourSystem.Instance.ExecuteLateUpdate();
        CoroutineSystem.Instance.YieldUpdate();
        BehaviourSystem.Instance.ExecuteOnDisable();
        BehaviourSystem.Instance.ExecuteOnDestroy();
        CoroutineSystem.Instance.YieldUntil();
        InputSystem.Instance.EndFrame();
    }

    public void OnRender(double deltaTime)
    {
        
    }
    
    public int TargetFrameRate
    {
        get => (int)_window.View.FramesPerSecond;
        set => _window.View.FramesPerSecond = Math.Max(0, value);
    }

    public bool VSync
    {
        get => _window.View.VSync;
        set => _window.View.VSync = value;
    }

    public bool IsRunning()
    {
        return !_window.View.IsClosing;
    }

    public void RequestClose()
    {
        _window.View.Close();
    }
}