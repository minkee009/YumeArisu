using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Systems;

namespace YumeArisu.Desktop.ImGuiExtension;

public class DebuggingUI
{
    DebuggingUIRegistry _registry;
    ImGuiController _controller;
    List<IImGuiWindow> _windows;

    public DebuggingUI(GL gl, IView view, IInputContext ctx)
    {
        _controller = new ImGuiController(
        gl,
        view,
        ctx,
        onConfigureIO: InitializeFont);

        InitializeStyle();

        _registry = new DebuggingUIRegistry();

        _windows = [new HierarchyWindow(), new InspectorWindow(), new TimeWindow()];

        foreach (var window in _windows)
            window.Initialize(_registry);
    }

    private void InitializeFont()
    {
        var io = ImGui.GetIO();
        string fontPath = FindKoreanFontPath();

        if (File.Exists(fontPath))
        {
            IntPtr glyphRanges = io.Fonts.GetGlyphRangesKorean();
            io.Fonts.AddFontFromFileTTF(fontPath, 18.0f, null, glyphRanges);
            System.Console.WriteLine("한글 폰트 불러오기 성공!");
        }
        else
        {
            io.Fonts.AddFontDefault();
        }
    }

    private void InitializeStyle()
    {
        var style = ImGui.GetStyle();
        style.FrameRounding = 6.0f;
        style.WindowRounding = 6.0f;
        style.GrabRounding = 6.0f;
        style.ScrollbarRounding = 6.0f;
    }

    public void Update(float deltaTime)
    {
        _controller.Update(deltaTime);
    }

    public void Render()
    {
        DrawMainMenuBar();

        foreach (var window in _windows)
            window.Render();

        _controller.Render();
    }

    private void DrawMainMenuBar()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.0f));

        if (ImGui.BeginMainMenuBar())
        {
            DrawWindowMenu();
            DrawSceneMenu();
            DrawDisplayMenu();
            DrawDebugMenu();

            ImGui.EndMainMenuBar();
        }

        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    private void DrawWindowMenu()
    {
        if (ImGui.BeginMenu("Windows"))
        {
            foreach (var window in _windows)
            {
                bool isOpen = window.IsOpen;
                if (ImGui.MenuItem(window.DisplayName, null, isOpen))
                    window.IsOpen = !isOpen;
            }

            ImGui.EndMenu();
        }
    }

    private void DrawSceneMenu()
    {
        if (ImGui.BeginMenu("Scene"))
        {
            var sceneManifest = SceneSystem.Instance.GetSceneManifest();
            ImGui.TextDisabled($"Current : {SceneControl.CurrentScene?.GetType().Name ?? "None"}");
            ImGui.Separator();

            foreach(var scene in sceneManifest.DynamicScenes)
            {
                var sceneName = scene.GetType().Name;
                if (ImGui.MenuItem(sceneName)) SceneControl.ChangeScene(sceneName);
            }

            ImGui.EndMenu();
        }
    }

    private void DrawDisplayMenu()
    {
        if (ImGui.BeginMenu("Display"))
        {
            bool vsync = ApplicationControl.VSync;
            if (ImGui.Checkbox("VSync", ref vsync))
                ApplicationControl.VSync = vsync;

            ImGui.Separator();

            foreach (DisplayMode mode in Enum.GetValues<DisplayMode>())
            {
                bool selected = WindowControl.DisplayMode == mode;
                if (ImGui.MenuItem(mode.ToString(), null, selected))
                    WindowControl.DisplayMode = mode;
            }

            ImGui.EndMenu();
        }
    }

    private void DrawDebugMenu()
    {
        if (ImGui.BeginMenu("Debug"))
        {
            if (ImGui.MenuItem("Force GC.Collect"))
                GC.Collect(2, GCCollectionMode.Forced, blocking: true);

            ImGui.Separator();

            if (ImGui.MenuItem("Quit"))
                ApplicationControl.RequestClose();

            ImGui.EndMenu();
        }
    }

    private static string FindKoreanFontPath()
    {
        string[] candidates;

        if (OperatingSystem.IsWindows())
        {
            candidates = new[]
            {
                @"C:\Windows\Fonts\malgun.ttf",
                @"C:\Windows\Fonts\malgunbd.ttf",
            };
        }
        else if (OperatingSystem.IsMacOS())
        {
            candidates = new[]
            {
                "/System/Library/Fonts/AppleSDGothicNeo.ttc",
                "/Library/Fonts/AppleGothic.ttf",
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            candidates = new[]
            {
                "/usr/share/fonts/truetype/nanum/NanumGothic.ttf",
                "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
                "/usr/share/fonts/noto-cjk/NotoSansCJK-Regular.ttc",
            };
        }
        else
        {
            candidates = Array.Empty<string>();
        }

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }
}