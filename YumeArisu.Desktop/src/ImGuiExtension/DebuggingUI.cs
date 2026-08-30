using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;

namespace YumeArisu.Desktop.ImGuiExtension;

public class DebuggingUI
{
    private DebuggingUIRegistry _registry;
    private ImGuiController _controller;
    private List<IImGuiWindow> _windows;
    private bool _showMenuBar;

    public DebuggingUI(GL gl, IView view, IInputContext ctx)
    {
        _controller = new ImGuiController(
        gl,
        view,
        ctx,
        onConfigureIO: InitializeFont);

        InitializeStyle();

        _registry = new DebuggingUIRegistry();
        _showMenuBar = true;
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
        style.PopupRounding = 6.0f;

        RangeAccessor<Vector4> colors = style.Colors;

        colors[(int)ImGuiCol.WindowBg]           = new Vector4(0.10f, 0.10f, 0.12f, 0.95f);
        colors[(int)ImGuiCol.Header]             = new Vector4(0.18f, 0.20f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.HeaderHovered]      = new Vector4(0.24f, 0.27f, 0.34f, 1.00f);
        colors[(int)ImGuiCol.HeaderActive]       = new Vector4(0.00f, 0.58f, 0.80f, 1.00f); // 클릭/선택 색상

        colors[(int)ImGuiCol.FrameBg]            = new Vector4(0.15f, 0.16f, 0.19f, 1.00f);
        colors[(int)ImGuiCol.FrameBgHovered]     = new Vector4(0.20f, 0.22f, 0.27f, 1.00f);
        colors[(int)ImGuiCol.Button]             = new Vector4(0.18f, 0.20f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.ButtonHovered]      = new Vector4(0.00f, 0.58f, 0.80f, 1.00f);
    }

    public void Update(float deltaTime)
    {
        _controller.Update(deltaTime);
    }

    public void Render()
    {
        if(_showMenuBar)
            DrawMainMenuBar();

        foreach (var window in _windows)
            window.Render();

        HandleDeselectClick();
        
        DrawSelectionOutline();
        DrawDebuggingUIStatus();

        _controller.Render();
    }

    public void ToggleShowMainMenuBar() => _showMenuBar = !_showMenuBar;

    private void HandleDeselectClick()
    {
        var io = ImGui.GetIO();

        if (io.WantCaptureMouse)
            return;

        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            _registry.ClearData(DebuggingUIKeys.SelectedGameObject);
    }

    private void DrawDebuggingUIStatus()
    {
        var io = ImGui.GetIO();
        var displaySize = io.DisplaySize;

        const string label = "Debugging UI : ON";
        const float padding = 10f;

        Vector2 textSize = ImGui.CalcTextSize(label);
        Vector2 textPos = new Vector2(
            displaySize.X - textSize.X - padding,
            displaySize.Y - textSize.Y - padding
        );

        var drawList = ImGui.GetForegroundDrawList();

        // 가독성을 위한 그림자(외곽선) 효과
        uint shadowColor = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.8f));
        drawList.AddText(textPos + new Vector2(1, 1), shadowColor, label);

        uint textColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)); 
        drawList.AddText(textPos, textColor, label);
    }

    private void DrawSelectionOutline()
    {
        var selected = _registry.GetData<GameObject>(DebuggingUIKeys.SelectedGameObject);
        if (selected is null || selected.IsDestroyed)
            return;

        var cameras = RenderSystem.Instance.GetActiveCameras();
        if (cameras.Count == 0)
            return;

        var camera = cameras.OrderByDescending(c => c.Depth).First();

        var screenPos = camera.WorldToScreenPoint(selected.Transform.WorldPosition);
        if (screenPos is not Vector2 sp)
            return;

        const float radius = 30f;
        var drawList = ImGui.GetBackgroundDrawList();

        // 0~1 사이를 부드럽게 오가는 값 (사인파, 주기 약 2초)
        float t = (MathF.Sin(Time.TotalTime * MathF.PI) + 1f) * 0.5f;

        var colorA = new Vector4(1f, 0.85f, 0.2f, 1f);   // 기존 노란색
        var colorB = new Vector4(0.6f, 1f, 0.6f, 1f);    // 연한 연두색
        var lerped = Vector4.Lerp(colorA, colorB, t);

        uint outlineColor = ImGui.GetColorU32(lerped);
        uint textColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f));

        drawList.AddCircle(sp, radius, outlineColor, 0, 2f);

        string label = selected.Name;
        Vector2 textSize = ImGui.CalcTextSize(label);
        Vector2 textPos = new Vector2(sp.X - textSize.X * 0.5f, sp.Y + radius + 4f);

        drawList.AddText(textPos, textColor, label);
    }

    private void DrawMainMenuBar()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, new Vector4(0.1f, 0.1f, 0.1f, 0.0f));

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
            var dynamicSceneNames = SceneSystem.Instance.DynamicSceneNames;
            ImGui.TextDisabled($"Current : {SceneControl.CurrentScene?.GetType().Name ?? "None"}");
            ImGui.Separator();

            foreach(var scene in dynamicSceneNames)
            {
                if (ImGui.MenuItem(scene)) SceneControl.ChangeScene(scene);
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
                "/usr/share/fonts/naver-nanum-gothic-fonts/NanumGothic.ttf",
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