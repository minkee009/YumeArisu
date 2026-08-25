using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Systems;

public class RenderSystem : SystemBase<RenderSystem, IView>
{
    public Vector2D<int> FramebufferSize { get; private set; }
    public float FramebufferAspect => FramebufferSize.X / FramebufferSize.Y;
    
    // GL Context
    private GL _gl;
    private ShaderBackend _shaderBackend;

    // Render Pipeline Object
    private List<Camera> _cameras;
    private List<Renderer> _renderers;
    private bool _needCamDepthSort;
    private bool _needRenderOrderSort;

    internal override void OnStartUp(IView view)
    {
        try
        {
            _gl = view.CreateOpenGL();
        }
        catch
        {
            Environment.FailFast("GL 컨텍스트를 생성하지 못했습니다.");
            return; // Exit가 비동기 콜백 안에서 즉시 안 먹힐 상황 대비한 안전장치
        }


        _cameras = new List<Camera>();
        _renderers = new List<Renderer>();

        _shaderBackend = view.API.API == ContextAPI.OpenGL ? ShaderBackend.OpenGLCore : ShaderBackend.OpenGLES;

        BuiltInRenderResources.Load();
    }

    internal override void OnShutDown()
    {
        BuiltInRenderResources.Unload();

        _cameras.Clear();
        _renderers.Clear();
        _cameras = null;
        _renderers = null;
        _gl = null;
    }

    public void OnFramebufferResize(Vector2D<int> size)
    {
        FramebufferSize = size;
        foreach (var cam in _cameras)
            cam.MarkProjectionMatrixDirty();
    }

    public void BeginFrame()
    {
        _gl.ClearColor(0,0,0,1.0f);
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        _gl.Viewport(FramebufferSize);

        // 렌더 오브젝트 정렬
        if (_needCamDepthSort)
        {
            _cameras.Sort((a,b) => a.Depth.CompareTo(b.Depth));
            _needCamDepthSort = false;
        }

        if (_needRenderOrderSort)
        {
            _renderers.Sort((a,b) => a.RenderOrder.CompareTo(b.RenderOrder));
            _needRenderOrderSort = false;
        }
    }

    public void Render()
    {
        // 활성 카메라 렌더링
        foreach(var cam in _cameras)
        {
            if (!cam.IsActiveAndEnabled)
                continue;

            // 'Draw Sprite' 스테이지
            var spriteMat = BuiltInRenderResources.DefaultSpriteMaterial;
            spriteMat.SetVector3($"{GlobalUniform.CameraPosition}", cam.Transform.WorldPosition);
            spriteMat.SetMatrix4x4($"{GlobalUniform.View}",  cam.ViewMatrix);
            spriteMat.SetMatrix4x4($"{GlobalUniform.Projection}", cam.ProjectionMatrix);

            foreach(var renderer in _renderers)
            {
                if (renderer.Enabled && renderer.GameObject.ActiveInHierarchy)
                {
                    spriteMat.SetMatrix4x4($"{GlobalUniform.Model}", renderer.Transform.WorldMatrix);
                    renderer.Draw();
                }
            }
        }
    }

    public void EndFrame()
    {
        _gl.Viewport(FramebufferSize);
    }

    public GL GetGL() => _gl;

    internal void RegisterCamera(Camera camera)
    {
        _cameras.Add(camera);
        _needCamDepthSort = true;
    } 

    internal void RegisterRenderer(Renderer renderer)
    {
        _renderers.Add(renderer);
        _needRenderOrderSort = true;
    } 

    internal void UnregisterCamera(Camera camera) => _cameras.Remove(camera);

    internal void UnregisterRenderer(Renderer renderer) => _renderers.Remove(renderer);

    internal ShaderBackend GetShaderBackend() => _shaderBackend;
}

// 문법 설탕용 클래스
public static class Screen
{
    public static Vector2D<int> Resolution => RenderSystem.Instance.FramebufferSize;
    public static float Aspect => RenderSystem.Instance.FramebufferAspect;
}