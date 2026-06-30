using System;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using YumeAris.Core.System;
using YumeAris.Core.Utility;

namespace YumeAris.Core;

public class Program
{
    private static IWindow window;
    private static GL gl;

    public static void Main(string[] args)
    {

        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(800, 600);
        options.Title = "YWMA - Yume Wo Miru Arisu";

        window = Window.Create(options);
        window.Update += dt => OnUpdate(dt);
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Run();
    }

    private static void OnUpdate(double dt)
    {
        if(Input.GetKeyDown(Key.Escape))
            window?.Close();
        InputSystem.Instance.EndFrame();
    }

    private static void OnLoad()
    {
        ConsoleColorExtensions.WriteLineColored($"{window.API.Version} : 윈도우가 올바르게 초기화 되었습니다.", ConsoleColor.Green);
        InputSystem.Instance.StartUp(window);

        gl = GL.GetApi(window);
        // Initialize OpenGL resources here
    }

    private static void OnRender(double deltaTime)
    {
        gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        // Render your scene here
    }
}
