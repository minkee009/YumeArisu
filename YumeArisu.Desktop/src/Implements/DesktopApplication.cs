using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Input;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Abstractions;
using YumeArisu.Game.SceneManifests;

namespace YumeArisu.Desktop.Implements;

public sealed class DesktopApplication : IApplicationControl
{
    private DesktopWindow _window;
#if !DEBUG
    private DesktopFileIO _fileIO;
#else
    private DebugFileIO _fileIO;
#endif

    public DesktopApplication(string title, int width, int height)
    {
        _window = new(title, width, height);
        _window.View.Load += OnLoad;
        _window.View.FramebufferResize += OnFramebufferResize;
        _window.View.Resize += OnResize;
        _window.View.Update += OnUpdate;
        _window.View.Render += OnRender;
        _window.View.Closing += OnClosing;
        _window.View.ShouldSwapAutomatically = false;

        _fileIO = new();
    }

    public void Run()
    {
        _window.View.Run();
    }

    public void OnLoad()
    {
#if !DEBUG
        _fileIO.Open("./Data", "dat");
#else
        _fileIO.SetRootFolder("./Assets");
#endif

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

        InputSystem.Instance.RegisterSystemKeyCombo(
            [Key.AltLeft], 
            Key.Enter, 
            _window.SwitchDisplayMode);
    }

    public void OnFramebufferResize(Vector2D<int> newSize)
    {
        // 물리적 : 내부 프레임버퍼 크기 변경 시
        RenderSystem.Instance.OnFramebufferResize(newSize);
        //System.Console.WriteLine($"Physical Screen Size : {newSize}");
    }

    public void OnResize(Vector2D<int> newSize)
    {
        // 논리적 : 윈도우 핸들 크기 변경 시 (DPI 있음)
        InputSystem.Instance.OnViewResize(newSize);
        //PointerEventSystem.Instance.OnScreenResize(newSize);
        //TouchSystem.Instance.OnPanelResize(newSize);
        //System.Console.WriteLine($"Logical Screen Size : {newSize}");
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
        
        _window.Present();
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
    
#if !DEBUG
        _fileIO.Close();
#endif
    }
    
    public int TargetFrameRate
    {
        get => (int)_window.View.FramesPerSecond;
        set
        {
            _window.View.FramesPerSecond = Math.Max(0, value);
            _window.View.UpdatesPerSecond = Math.Max(0, value);
        }
    }

    public bool VSync
    {
        get => _window.View.VSync;
        set => _window.View.VSync = value;
    }

    public bool IsRunning() => !_window.View.IsClosing;

    public void RequestClose() => _window.View.Close();
}