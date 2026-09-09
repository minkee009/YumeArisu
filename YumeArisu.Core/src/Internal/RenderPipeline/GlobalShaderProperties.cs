using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal static class GlobalShaderProperties
{
    internal static uint Handle { get; private set; }
    internal const string View = "uView";
    internal const string Projection = "uProjection";
    internal const string CameraPosition = "uCameraPosition";

    private static CameraBlock _cameraBlock;

    internal const string CameraBlockSource = $$"""
        layout(std140) uniform CameraBlock
        {
            mat4 {{View}};
            mat4 {{Projection}};
            vec3 {{CameraPosition}};
        };
        
    """;

    internal const string VertexSource = $$"""

        """;

    internal const string FragmentSource = $$"""

        """;

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    private struct CameraBlock
    {
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Vector3 CameraPosition;
        private float _padding;
    }

    internal static void Initialize()
    {
        var gl = RenderSystem.Instance.GetGL();

        Handle = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.UniformBuffer, Handle);
        gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)Unsafe.SizeOf<CameraBlock>(), in IntPtr.Zero, BufferUsageARB.DynamicDraw);
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 0, Handle);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
    }

    internal static void UpdateCamera(in Matrix4x4 view, in Matrix4x4 projection, in Vector3 cameraPosition)
    {
        _cameraBlock.View = view;
        _cameraBlock.Projection = projection;
        _cameraBlock.CameraPosition = cameraPosition;
        Upload();
    }

    internal static void Release()
    {
        if (Handle == 0)
            return;

        var gl = RenderSystem.Instance.GetGL();
        gl.DeleteBuffer(Handle);
        Handle = 0;
    }

    private static void Upload()
    {
        if (Handle == 0)
            return;

        var gl = RenderSystem.Instance.GetGL();
        gl.BindBuffer(BufferTargetARB.UniformBuffer, Handle);

        unsafe
        {
            fixed (CameraBlock* ptr = &_cameraBlock)
            {
                gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (nuint)Unsafe.SizeOf<CameraBlock>(), ptr);
            }
        }

        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
    }
}