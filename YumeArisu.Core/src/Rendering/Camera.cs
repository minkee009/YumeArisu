using System.Numerics;
using Silk.NET.Maths;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Core.Rendering;

public class Camera : Behaviour
{
    /// <summary>
    /// 멀티플 카메라의 정렬 순서입니다. 값이 클수록 마지막에 렌더링됩니다.
    /// </summary>
    public int Depth { get; set; } = 0;
    
    /// <summary>
    /// 스크린 대상에 대한 카메라의 정규화된 뷰포트 영역입니다.
    /// 좌하단(0,0) ~ 우상단(1,1) 기준이며, 각 값은 0~1 범위입니다.
    /// </summary>
    public Rectangle<float> ViewRect { get; set; } = new(0, 0, 1, 1);

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

    private Matrix4x4 _cachedViewMatrix;
    private bool _viewMatrixDirty = true;

    protected internal override void OnAttach()
    {
        GameObject.Transform.OnWorldMatrixDirty += HandleTransformMatrixChange;
    }

    protected internal override void OnDetach()
    {
        GameObject.Transform.OnWorldMatrixDirty -= HandleTransformMatrixChange;
    }

    private void HandleTransformMatrixChange(Transform t)
    {
        _viewMatrixDirty = true;
    }
}