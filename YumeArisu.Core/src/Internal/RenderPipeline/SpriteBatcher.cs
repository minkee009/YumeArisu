using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Internal.RenderPipeline;

using SpriteTexture = Rendering.Texture;

internal sealed class SpriteBatcher : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SpriteInstance
    {
        public Matrix4x4 Model;
        public Vector2 Size;
        public Vector2 Pivot;
        public Vector4 UVRect;
        public Vector4 Color;
        public float Depth;
    }

    private sealed class SpriteBatch
    {
        internal SpriteTexture Texture { get; }
        internal BlendMode BlendMode { get; }
        internal List<SpriteInstance> Instances { get; } = new();

        internal SpriteBatch(SpriteTexture texture, BlendMode blendMode)
        {
            Texture = texture;
            BlendMode = blendMode;
        }
    }

    private readonly List<SpriteBatch> _batches = new();
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

            gl.BindBuffer(GLEnum.ArrayBuffer, _instanceBuffer);
            uint stride = (uint)Marshal.SizeOf<SpriteInstance>();

            ConfigureMatrixAttribute(2, stride, 0);
            ConfigureMatrixAttribute(3, stride, 16);
            ConfigureMatrixAttribute(4, stride, 32);
            ConfigureMatrixAttribute(5, stride, 48);

            ConfigureAttribute(6, 2, stride, 64);
            ConfigureAttribute(7, 2, stride, 72);
            ConfigureAttribute(8, 4, stride, 80);
            ConfigureAttribute(9, 4, stride, 96);
        }

        gl.BindVertexArray(0);
        _initialized = true;
    }

    internal void Submit(SpriteTexture texture, Matrix4x4 model, Vector2 size, Vector2 pivot,
        Vector4 uvRect, Vector4 color, BlendMode blendMode, float depth)
    {
        SpriteBatch batch;
        if (_batches.Count == 0 || _batches[^1].Texture != texture || _batches[^1].BlendMode != blendMode)
        {
            batch = new SpriteBatch(texture, blendMode);
            _batches.Add(batch);
        }
        else
            batch = _batches[^1];

        batch.Instances.Add(new SpriteInstance
        {
            Model = model,
            Size = size,
            Pivot = pivot,
            UVRect = uvRect,
            Color = color,
            Depth = depth
        });
    }

    internal unsafe void Flush()
    {
        if (!_initialized)
            return;

        var gl = RenderSystem.Instance.GetGL();
        var shader = BuiltInRenderResources.DefaultSpriteShader;

        BuiltInRenderResources.DefaultSpriteMaterial.ApplyRenderState();
        shader.Use();
        shader.SetTextureUnit("MainTexture", 0);

        gl.BindVertexArray(_vao);

        foreach (var batch in _batches)
        {
            int count = batch.Instances.Count;
            if (count == 0)
                continue;

            if (batch.BlendMode != BlendMode.Opaque)
                batch.Instances.Sort(CompareDepth);

            BuiltInRenderResources.DefaultSpriteMaterial.ApplyRenderState(batch.BlendMode);
            batch.Texture.Bind();
            gl.BindBuffer(GLEnum.ArrayBuffer, _instanceBuffer);

            ReadOnlySpan<SpriteInstance> span = CollectionsMarshal.AsSpan(batch.Instances);
            uint requiredSize = (uint)(count * Marshal.SizeOf<SpriteInstance>());

            fixed (SpriteInstance* ptr = span)
            {
                if (requiredSize > _allocatedBufferSize)
                {
                    _allocatedBufferSize = Math.Max(requiredSize, _allocatedBufferSize * 2);
                    gl.BufferData(GLEnum.ArrayBuffer, _allocatedBufferSize, ptr, GLEnum.StreamDraw);
                }
                else
                    gl.BufferSubData(GLEnum.ArrayBuffer, 0, requiredSize, ptr);
            }

            gl.DrawElementsInstanced(
                GLEnum.Triangles,
                BuiltInRenderResources.DefaultQuadMesh.IndexCount,
                DrawElementsType.UnsignedInt,
                null,
                (uint)count);

            batch.Instances.Clear();
        }

        gl.BindVertexArray(0);
        _batches.Clear();
    }

    private static int CompareDepth(SpriteInstance left, SpriteInstance right)
    {
        int result = left.Depth.CompareTo(right.Depth);
        if (result != 0)
            return result;

        return 0;
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
