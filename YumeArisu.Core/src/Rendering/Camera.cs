using System.Numerics;
using Silk.NET.Maths;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;
using Silk.NET.Vulkan;

namespace YumeArisu.Core.Rendering;

public class Camera : Behaviour
{
    public ProjectionMode ProjectionMode { get; set; } = ProjectionMode.Orthogonal;

    /// <summary>
    /// 멀티플 카메라의 정렬 순서입니다. 값이 클수록 마지막에 렌더링됩니다.
    /// </summary>
    public int Depth { get; set; } = 0;
    
    /// <summary>
    /// 스크린 대상에 대한 카메라의 정규화된 뷰포트 영역입니다.
    /// 좌하단(0,0) ~ 우상단(1,1) 기준이며, 각 값은 0~1 범위입니다.
    /// </summary>
    public Rectangle<float> ViewRect { get; set; } = new(0, 0, 1, 1);

    public float NearPlane { get; set; } = -10.0f;

    public float FarPlane { get; set; } = 10.0f;

    public float Size { get; set; } = 5;

    public float FieldOfView { get; set; } = 60;

    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (_viewMatrixDirty)
            {
                Matrix4x4.Invert(GameObject.Transform.WorldMatrix, out var inverse);
                _cachedViewMatrix = inverse;
                _viewMatrixDirty = false;
            }
            return _cachedViewMatrix;
        }
    }

    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (_projMatrixDirty)
            {
                var framebufferAspect = RenderSystem.Instance.FramebufferAspect;
                switch(ProjectionMode)
                {
                    case ProjectionMode.Orthogonal:
                        float height = Size * 2.0f;
                        float width = height * framebufferAspect;
                        _cachedProjMatrix = Matrix4x4.CreateOrthographic(height, width, NearPlane, FarPlane);
                        break;
                    case ProjectionMode.Perspective:
                        _cachedProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, framebufferAspect, NearPlane, FarPlane);
                        break;
                }
                _projMatrixDirty = false;
            }

            return _cachedProjMatrix;
        }
    }

    private Matrix4x4 _cachedViewMatrix;
    private Matrix4x4 _cachedProjMatrix;
    private bool _viewMatrixDirty = true;
    private bool _projMatrixDirty = true;

    protected internal override void OnAttach()
    {
        GameObject.Transform.WorldMatrixDirtyChange += HandleTransformMatrixChange;
        RenderSystem.Instance.RegisterCamera(this);
    }

    protected internal override void OnDetach()
    {
        GameObject.Transform.WorldMatrixDirtyChange -= HandleTransformMatrixChange;
    }

    private void HandleTransformMatrixChange(Transform t) => MarkViewMatrixDirty();

    internal void MarkViewMatrixDirty() => _viewMatrixDirty = true;

    internal void MarkProjectionMatrixDirty() => _projMatrixDirty = true;
}