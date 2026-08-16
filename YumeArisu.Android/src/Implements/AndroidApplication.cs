using Android.Content;
using Android.Content.Res;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Systems;
using YumeArisu.Game.SceneManifests;

namespace YumeArisu.Android.Implements;

public sealed class AndroidApplication : IApplicationControl
{
    private AndroidWindow _window;
    private AndroidFileIO _fileIO;

    public AndroidApplication(AssetManager assets, Context context)
    {
        _window = new();
        _fileIO = new(assets, context);
    }

    public void Run()
    {
        _window.View.Load += OnLoad;
        _window.View.FramebufferResize += OnFramebufferResize;
        _window.View.Update += OnUpdate;
        _window.View.Render += OnRender;
        _window.View.Closing += OnClosing;

        _window.View.Run();

        _window.View.Dispose();
    }

    public void OnLoad()
    {
        _fileIO.Open("Data", "dat");

        ApplicationSystem.Instance.StartUp(this);
        WindowSystem.Instance.StartUp(_window);
        TimeSystem.Instance.StartUp(default);
        InputSystem.Instance.StartUp(_window.View);
        RenderSystem.Instance.StartUp(_window.View);
        ResourceSystem.Instance.StartUp(_fileIO);
        BehaviourSystem.Instance.StartUp(default);
        CoroutineSystem.Instance.StartUp(default);
        SceneSystem.Instance.StartUp(new TestSceneManifest());

        SceneSystem.Instance.BeforeSceneChange += CoroutineSystem.Instance.ImmediateStopAllCoroutines;
        
        OnFramebufferResize(_window.View.FramebufferSize);
        OnResize(_window.View.Size);
    }

    public void OnFramebufferResize(Vector2D<int> newSize)
    {
        RenderSystem.Instance.OnFramebufferResize(newSize);
    }

    public void OnResize(Vector2D<int> newSize)
    {
        InputSystem.Instance.OnResize(newSize);
    }

    public void OnUpdate(double deltaTime)
    {
        TimeSystem.Instance.BeginFrame(deltaTime);
        SceneSystem.Instance.BeginFrame();
        BehaviourSystem.Instance.BeginFrame();
        BehaviourSystem.Instance.ExecuteOnAwake();
        BehaviourSystem.Instance.ExecuteOnEnable();
        BehaviourSystem.Instance.ExecuteOnStart();
        TimeSystem.Instance.ConsumeFixedSteps(
            (_) => 
            {
                BehaviourSystem.Instance.ExecuteOnFixedUpdate();
                CoroutineSystem.Instance.YieldFixedUpdate();
            });
        BehaviourSystem.Instance.ExecuteOnUpdate();
        BehaviourSystem.Instance.ExecuteOnLateUpdate();
        CoroutineSystem.Instance.YieldUpdate();
        BehaviourSystem.Instance.ExecuteOnDisable();
        BehaviourSystem.Instance.ExecuteOnRemove();
        CoroutineSystem.Instance.YieldUntil();
        InputSystem.Instance.EndFrame();
    }

    public void OnRender(double deltaTime)
    {
        RenderSystem.Instance.BeginFrame();
        RenderSystem.Instance.Render();
        RenderSystem.Instance.EndFrame();
    }

    public void OnClosing()
    {
        SceneSystem.Instance.BeforeSceneChange -= CoroutineSystem.Instance.ImmediateStopAllCoroutines;

        SceneSystem.Instance.ShutDown();
        CoroutineSystem.Instance.ShutDown();
        BehaviourSystem.Instance.ShutDown();
        TimeSystem.Instance.ShutDown();
        ResourceSystem.Instance.ShutDown();
        RenderSystem.Instance.ShutDown();
        InputSystem.Instance.ShutDown();
        WindowSystem.Instance.ShutDown();
        ApplicationSystem.Instance.ShutDown();
        
        _fileIO.Close();
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

    public bool IsRunning() => !_window.View.IsClosing;

    public void RequestClose() => _window.View.Close();
}