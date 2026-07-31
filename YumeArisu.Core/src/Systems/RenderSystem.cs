using System.Drawing;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class RenderSystem : SystemBase<RenderSystem, IView>
{
    private GL _gl;

    internal override void OnStartUp(IView view)
    {
        _gl = GL.GetApi(view);
        if(_gl == null)
            throw new Exception("그래픽 라이브러리를 초기화 하지 못했습니다.");

        view.FramebufferResize += size => _gl.Viewport(size);
    }

    internal override void OnShutDown()
    {
        _gl = null;
    }

    public void BeginFrame()
    {
        // Camera Clear
        _gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
        _gl.Clear((uint) ClearBufferMask.ColorBufferBit);
    }

    public void Render()
    {
        
    }

    public void EndFrame()
    {
        
    }

    public GL GetGL() => _gl;
}