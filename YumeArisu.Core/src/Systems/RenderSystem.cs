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
    private List<SpriteRenderer> _spriteRenderers;
    private SpriteBatcher _spriteBatcher;
    private bool _needCamDepthSort;
    private bool _needRenderOrderSort;
    private long _nextRendererRegistrationOrder;

    internal override void OnStartUp(GL gl)
    {
        _gl = gl;

        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(GLEnum.Less); // 표준: 더 작은 Z(더 가까운)가 이김

        _cameras = new List<Camera>();
        _spriteRenderers = new List<SpriteRenderer>();

        _shaderBackend = DetectShaderBackend(_gl);

        GlobalUniform.Initialize();
        BuiltInRenderResources.Load();
        _spriteBatcher = new SpriteBatcher();
    }

    internal override void OnShutDown()
    {
        BuiltInRenderResources.Unload();
        GlobalUniform.Release();

        _spriteBatcher?.Dispose();

        _cameras.Clear();
        _spriteRenderers.Clear();
        _cameras = null;
        _spriteRenderers = null;
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
            _spriteRenderers.Sort(CompareRenderers);
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

            var viewportX = (int)(FramebufferSize.X * cam.ViewRect.Origin.X);
            var viewportY = (int)(FramebufferSize.Y * cam.ViewRect.Origin.Y);
            var viewportWidth = (uint)(FramebufferSize.X * cam.ViewRect.Size.X);
            var viewportHeight = (uint)(FramebufferSize.Y * cam.ViewRect.Size.Y);

            _gl.Viewport(viewportX, viewportY, viewportWidth, viewportHeight);
            _gl.Clear((uint)ClearBufferMask.DepthBufferBit);

            // 'Draw Sprite' 스테이지
            {
                GlobalUniform.UpdateCamera(cam.ViewMatrix, cam.ProjectionMatrix, cam.Transform.WorldPosition);

                foreach (var renderer in _spriteRenderers)
                {
                    if (renderer.Enabled && renderer.GameObject.ActiveInHierarchy)
                    {
                        if (renderer.UsesImmediateDraw)
                            _spriteBatcher.Flush();

                        renderer.Draw();
                    }
                }

                _spriteBatcher.Flush();
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

    internal void RegisterSpriteRenderer(SpriteRenderer renderer)
    {
        renderer.Batcher = _spriteBatcher;
        renderer.RegistrationOrder = _nextRendererRegistrationOrder++;
        renderer.IsRegistered = true;
        _spriteRenderers.Add(renderer);
        _needRenderOrderSort = true;
    } 

    internal void UnregisterSpriteRenderer(SpriteRenderer renderer)
    {
        renderer.IsRegistered = false;
        _spriteRenderers.Remove(renderer);
    }

    internal void MarkRenderOrderDirty() => _needRenderOrderSort = true;

    internal void UnregisterCamera(Camera camera) => _cameras.Remove(camera);

    internal ShaderBackend GetShaderBackend() => _shaderBackend;

    private static int CompareRenderers(SpriteRenderer left, SpriteRenderer right)
    {
        int result = GetRenderQueue(left).CompareTo(GetRenderQueue(right));
        if (result != 0)
            return result;

        result = left.RenderOrder.CompareTo(right.RenderOrder);
        if (result != 0)
            return result;

        return left.RegistrationOrder.CompareTo(right.RegistrationOrder);
    }

    private static int GetRenderQueue(SpriteRenderer renderer)
    {
        if (renderer.RenderBlendMode == BlendMode.Opaque)
            return Rendering.RenderQueue.Geometry;

        return renderer.Material.RenderQueue;
    }

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