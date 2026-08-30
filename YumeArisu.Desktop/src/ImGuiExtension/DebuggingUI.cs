using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using YumeArisu.Core.Systems;

namespace YumeArisu.Desktop.ImGuiExtension;

public class DebuggingUI
{
    ImGuiController _controller;

    public DebuggingUI(GL gl, IView view, IInputContext ctx)
    {
        _controller = new ImGuiController(
        gl,
        view,
        ctx,
        onConfigureIO: () =>
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
        });

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
        ImGui.Begin($"SceneInfo : \"{SceneControl.CurrentScene.GetType().Name}\"");

        foreach (var go in SceneControl.CurrentScene.GameObjects)
        {
            ImGui.Text(go.Name);
        }

        ImGui.End();
        
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