using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;

namespace YWMA.Core;

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
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Run();
    }

    private static void OnLoad()
    {
        gl = GL.GetApi(window);
        // Initialize OpenGL resources here
    }

    private static void OnRender(double deltaTime)
    {
        gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        // Render your scene here
    }
}
