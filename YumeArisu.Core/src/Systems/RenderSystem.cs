using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Systems;

public class RenderSystem : SystemBase<RenderSystem, GL>
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

    internal override void OnStartUp(GL gl)
    {
        _gl = gl;

        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(GLEnum.Less); // 표준: 더 작은 Z(더 가까운)가 이김

        _cameras = new List<Camera>();
        _renderers = new List<Renderer>();

        _shaderBackend = DetectShaderBackend(_gl);

        GlobalUniform.Initialize();
        BuiltInRenderResources.Load();
    }

    internal override void OnShutDown()
    {
        BuiltInRenderResources.Unload();
        GlobalUniform.Release();

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
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
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
        foreach (var cam in _cameras)
        {
            if (!cam.IsActiveAndEnabled)
                continue;

            // 'Draw Sprite' 스테이지
            GlobalUniform.UpdateCamera(cam.ViewMatrix, cam.ProjectionMatrix, cam.Transform.WorldPosition);

            foreach (var renderer in _renderers)
            {
                if (renderer.Enabled && renderer.GameObject.ActiveInHierarchy)
                {
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

    private static ShaderBackend DetectShaderBackend(GL gl)
    {
        string version = gl.GetStringS(StringName.Version);
        return version.Contains("OpenGL ES") ? ShaderBackend.OpenGLES : ShaderBackend.OpenGLCore;
    }

    public List<Camera> GetActiveCameras()
    {
        List<Camera> activeCameras = new();
        foreach(var cam in _cameras)
        {
            if(cam.IsActiveAndEnabled)
                activeCameras.Add(cam);
        }

        return activeCameras;
    }
}

// 문법 설탕용 클래스
public static class Screen
{
    public static Vector2D<int> Resolution => RenderSystem.Instance.FramebufferSize;
    public static float Aspect => RenderSystem.Instance.FramebufferAspect;
}