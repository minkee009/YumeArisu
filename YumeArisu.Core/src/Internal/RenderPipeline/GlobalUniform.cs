using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal static class GlobalUniform
{
    internal static uint Handle { get; private set; }
    internal const string Model = "uModel";
    internal const string View = "uView";
    internal const string Projection = "uProjection";
    internal const string CameraPosition = "uCameraPosition";

    private static SceneBlock _sceneBlock;

    internal const string Source = $$"""
        layout(std140) uniform SceneBlock
        {
            mat4 {{Model}};
            mat4 {{View}};
            mat4 {{Projection}};
            vec3 {{CameraPosition}};
        };
    """;

    // std140 정렬 규격: vec3(12b) + float padding(4b) = 16b 맞춤
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    private struct SceneBlock
    {
        public Matrix4x4 Model;
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Vector3 CameraPosition;
        private float _padding; // 16바이트 정렬용 패딩
    }

    internal static void Initialize()
    {
        var gl = RenderSystem.Instance.GetGL();

        Handle = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.UniformBuffer, Handle);
        
        // Unsafe.SizeOf로 크기 계산 단순화
        gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)Unsafe.SizeOf<SceneBlock>(), in IntPtr.Zero, BufferUsageARB.DynamicDraw);
        
        // UBO를 Binding Point 0번에 고정 연결
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 0, Handle);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
    }

    internal static void UpdateCamera(in Matrix4x4 view, in Matrix4x4 projection, in Vector3 cameraPosition)
    {
        // HLSL/GLSL 행렬 전달을 위한 Transpose
        _sceneBlock.View = view;
        _sceneBlock.Projection = projection;
        _sceneBlock.CameraPosition = cameraPosition;
        Upload();
    }

    internal static void UpdateModel(in Matrix4x4 model)
    {
        _sceneBlock.Model = model;
        Upload();
    }

    internal static void Release()
    {
        if (Handle == 0) return;

        var gl = RenderSystem.Instance.GetGL();
        gl.DeleteBuffer(Handle);
        Handle = 0;
    }

    private static void Upload()
    {
        if (Handle == 0) return;

        var gl = RenderSystem.Instance.GetGL();
        gl.BindBuffer(BufferTargetARB.UniformBuffer, Handle);

        unsafe
        {
            fixed (SceneBlock* ptr = &_sceneBlock)
            {
                gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (nuint)Unsafe.SizeOf<SceneBlock>(), ptr);
            }
        }

        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
    }
}