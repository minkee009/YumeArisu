using System.Numerics;
using Silk.NET.Maths;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Core.Rendering;

public class Camera : Behaviour
{
    public int Depth { get; set; } = 0;
    public Rectangle<float> ViewRect { get; set; }
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

    protected internal override void OnAttached()
    {
        GameObject.Transform.OnWorldMatrixDirty += HandleTransformMatrixChange;
    }

    protected internal override void OnDetached()
    {
        GameObject.Transform.OnWorldMatrixDirty -= HandleTransformMatrixChange;
    }

    private void HandleTransformMatrixChange(Transform t)
    {
        _viewMatrixDirty = true;
    }
}