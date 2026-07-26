using Android.Content;
using Android.Content.Res;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Systems;
using YumeArisu.Game.Scenes.Manifset;

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
        // Included assets should be loaded with the help of Android.Content.Res.AssetManager.
        // The included example shaders and texture have build action of "AndroidAsset".

        _window.View.Load += OnLoad;
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
        ResourceSystem.Instance.StartUp(_fileIO);
        BehaviourSystem.Instance.StartUp(default);
        CoroutineSystem.Instance.StartUp(default);
        SceneSystem.Instance.StartUp(new TestSceneManifest());

        SceneSystem.Instance.OnBeforeSceneChange += CoroutineSystem.Instance.ImmediateStopAllCoroutines;
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

    public void OnClosing()
    {
        SceneSystem.Instance.OnBeforeSceneChange -= CoroutineSystem.Instance.ImmediateStopAllCoroutines;

        SceneSystem.Instance.ShutDown();
        CoroutineSystem.Instance.ShutDown();
        BehaviourSystem.Instance.ShutDown();
        TimeSystem.Instance.ShutDown();
        ResourceSystem.Instance.ShutDown();
        InputSystem.Instance.ShutDown();
        WindowSystem.Instance.ShutDown();
        ApplicationSystem.Instance.ShutDown();
        
        _fileIO.Close();
    }

    public int TargetFrameRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool VSync { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsRunning() => !_window.View.IsClosing;

    public void RequestClose() => _window.View.Close();
}