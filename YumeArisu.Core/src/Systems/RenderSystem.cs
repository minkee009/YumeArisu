using System.Drawing;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class RenderSystem : SystemBase<RenderSystem, IView>
{
    private GL _gl;

    internal override void OnStartUp(IView view)
    {
        try
        {
            _gl = GL.GetApi(view);
        }
        catch
        {
            Environment.FailFast("GL 컨텍스트를 찾지 못했습니다. 그래픽스 API는 OpenGL를 사용해야합니다.");
            return; // Exit가 비동기 콜백 안에서 즉시 안 먹힐 상황 대비한 안전장치
        }
    }

    internal override void OnShutDown()
    {
        _gl = null;

        // 카메라 리스트 해제
        // 렌더러 리스트 해제
    }

    public void OnFramebufferResize(Vector2D<int> size) => _gl.Viewport(size);

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