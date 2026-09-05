using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Systems;
using SpriteTexture = YumeArisu.Core.Rendering.Texture;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal sealed class SpriteBatcher : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SpriteInstance
    {
        public Matrix4x4 Model;   // Location 2, 3, 4, 5 (64 Bytes)
        public Vector2 Size;      // Location 6 (8 Bytes)
        public Vector2 Pivot;     // Location 7 (8 Bytes)
        public Vector4 UVRect;    // Location 8 (16 Bytes)
        public Vector4 Color;     // Location 9 (16 Bytes)
    }

    private readonly Dictionary<SpriteTexture, List<SpriteInstance>> _batches = new();
    private uint _instanceBuffer;
    private uint _vao;
    private uint _allocatedBufferSize;
    private bool _initialized;

    internal SpriteBatcher()
    {
        var gl = RenderSystem.Instance.GetGL();
        var quad = BuiltInRenderResources.DefaultQuadMesh;
        _vao = gl.GenVertexArray();
        _instanceBuffer = gl.GenBuffer();

        gl.BindVertexArray(_vao);
        gl.BindBuffer(GLEnum.ArrayBuffer, quad.VBOHandle);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, quad.EBOHandle);

        unsafe
        {
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 20, (void*)0);
            
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 20, (void*)12);

            // Instance VBO Attributes
            gl.BindBuffer(GLEnum.ArrayBuffer, _instanceBuffer);
            uint stride = (uint)Marshal.SizeOf<SpriteInstance>(); // Total: 112 Bytes

            // Matrix4x4 (Location 2 ~ 5)
            ConfigureMatrixAttribute(2, stride, 0);
            ConfigureMatrixAttribute(3, stride, 16);
            ConfigureMatrixAttribute(4, stride, 32);
            ConfigureMatrixAttribute(5, stride, 48);

            // Instance Attributes
            ConfigureAttribute(6, 2, stride, 64);  // Size
            ConfigureAttribute(7, 2, stride, 72);  // Pivot
            ConfigureAttribute(8, 4, stride, 80);  // UVRect
            ConfigureAttribute(9, 4, stride, 96);  // Color
        }

        gl.BindVertexArray(0);
        _initialized = true;
    }

    internal void Submit(SpriteTexture texture, Matrix4x4 model, Vector2 size, Vector2 pivot,
        Vector4 uvRect, Vector4 color)
    {
        if (!_batches.TryGetValue(texture, out var batch))
        {
            batch = new List<SpriteInstance>();
            _batches.Add(texture, batch);
        }

        batch.Add(new SpriteInstance
        {
            Model = model,
            Size = size,
            Pivot = pivot,
            UVRect = uvRect,
            Color = color
        });
    }

    internal unsafe void Flush()
    {
        if (!_initialized)
            return;

        var gl = RenderSystem.Instance.GetGL();
        var shader = BuiltInRenderResources.DefaultSpriteShader;

        shader.Use();
        shader.SetTextureUnit("MainTexture", 0);

        gl.BindVertexArray(_vao);

        foreach (var (texture, batch) in _batches)
        {
            int count = batch.Count;
            if (count == 0) continue;

            texture.Bind();
            gl.BindBuffer(GLEnum.ArrayBuffer, _instanceBuffer);

            ReadOnlySpan<SpriteInstance> span = CollectionsMarshal.AsSpan(batch);
            uint requiredSize = (uint)(count * Marshal.SizeOf<SpriteInstance>());

            fixed (SpriteInstance* ptr = span)
            {
                if (requiredSize > _allocatedBufferSize)
                {
                    _allocatedBufferSize = Math.Max(requiredSize, _allocatedBufferSize * 2);
                    gl.BufferData(GLEnum.ArrayBuffer, _allocatedBufferSize, ptr, GLEnum.StreamDraw);
                }
                else
                {
                    gl.BufferSubData(GLEnum.ArrayBuffer, 0, requiredSize, ptr);
                }
            }

            gl.DrawElementsInstanced(
                GLEnum.Triangles,
                BuiltInRenderResources.DefaultQuadMesh.IndexCount,
                DrawElementsType.UnsignedInt,
                null,
                (uint)count
            );

            batch.Clear();
        }

        gl.BindVertexArray(0);
        _batches.Clear();
    }

    public void Dispose()
    {
        if (!_initialized)
            return;

        var gl = RenderSystem.Instance.GetGL();
        gl.DeleteVertexArray(_vao);
        gl.DeleteBuffer(_instanceBuffer);
        
        _vao = 0;
        _instanceBuffer = 0;
        _initialized = false;
    }

    private static unsafe void ConfigureMatrixAttribute(uint location, uint stride, int offset)
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.EnableVertexAttribArray(location);
        gl.VertexAttribPointer(location, 4, GLEnum.Float, false, stride, (void*)offset);
        gl.VertexAttribDivisor(location, 1);
    }

    private static unsafe void ConfigureAttribute(uint location, int componentCount, uint stride, int offset)
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.EnableVertexAttribArray(location);
        gl.VertexAttribPointer(location, componentCount, GLEnum.Float, false, stride, (void*)offset);
        gl.VertexAttribDivisor(location, 1);
    }
}