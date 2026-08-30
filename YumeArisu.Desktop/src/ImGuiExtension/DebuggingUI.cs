using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

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

        _windows = [new HierarchyWindow(), new InspectorWindow()];

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
        foreach(var window in _windows)
            window.Render();

        _controller.Render();
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