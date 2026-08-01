using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;
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
    private ImGuiController _controller;
#endif

    public DesktopApplication(string title, int width, int height)
    {
        _window = new(title, width, height);
        _window.View.Load += OnLoad;
        _window.View.FramebufferResize += OnFrameBufferResized;
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
#if !DEBUG
        _fileIO.Open("./Data", "dat");
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

        SceneSystem.Instance.OnBeforeSceneChange += CoroutineSystem.Instance.ImmediateStopAllCoroutines;
#if DEBUG
        _controller = new ImGuiController(
            RenderSystem.Instance.GetGL(), 
            _window.View, 
            InputSystem.Instance.GetInputContext()
        );
#endif
        InputSystem.Instance.RegisterSystemKeyCombo(
            [Silk.NET.Input.Key.AltLeft], 
            Silk.NET.Input.Key.Enter, 
            _window.SwitchScreenMode
        );
    }

    public void OnFrameBufferResized(Vector2D<int> newSize)
    {
        // 물리적 : 내부 프레임버퍼 크기 변경 시
        RenderSystem.Instance.OnFramebufferResize(newSize);
    }

    public void OnResized(Vector2D<int> newSize)
    {
        // 논리적 : 윈도우 핸들 크기 변경 시
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
#if DEBUG
        _controller.Update((float)deltaTime);
#endif
        RenderSystem.Instance.BeginFrame();
        RenderSystem.Instance.Render();
#if DEBUG
        ImGuiNET.ImGui.ShowDemoWindow();
        _controller.Render();
#endif
        RenderSystem.Instance.EndFrame();
    }

    public void OnClosing()
    {
        SceneSystem.Instance.OnBeforeSceneChange -= CoroutineSystem.Instance.ImmediateStopAllCoroutines;

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