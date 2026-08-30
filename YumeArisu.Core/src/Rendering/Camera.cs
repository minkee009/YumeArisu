using System.Numerics;
using Silk.NET.Maths;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;
using Silk.NET.Vulkan;

namespace YumeArisu.Core.Rendering;

public class Camera : Behaviour
{
    public ProjectionMode ProjectionMode 
    { 
        get => _projectionMode; 
        set
        {
            if (_projectionMode == value) 
                return;
           
            _projectionMode = value;
            MarkProjectionMatrixDirty();
        } 
    }

    /// <summary>
    /// 멀티플 카메라의 정렬 순서입니다. 값이 클수록 마지막에 렌더링됩니다.
    /// </summary>
    public int Depth { get; set; } = 0;
    
    /// <summary>
    /// 스크린 대상에 대한 카메라의 정규화된 뷰포트 영역입니다.
    /// 좌하단(0,0) ~ 우상단(1,1) 기준이며, 각 값은 0~1 범위입니다.
    /// </summary>
    public Rectangle<float> ViewRect
    {
        get => _viewRect;
        set
        {
            if (_viewRect == value)
                return;

            _viewRect = value;
            MarkProjectionMatrixDirty();
        }
    }

    public float NearPlane
    {
        get => _near;
        set
        {
            if (_near == value)
                return;

            _near = value;
            MarkProjectionMatrixDirty();
        }
    }

    public float FarPlane
    {
        get => _far;
        set
        {
            if (_far == value)
                return;

            _far = value;
            MarkProjectionMatrixDirty();
        }
    }

    public float Size
    {
        get => _size;
        set
        {
            if (_size == value)
                return;

            _size = value;
            MarkProjectionMatrixDirty();
        }
    }

    public float FieldOfView
    {
        get => _fov;
        set
        {
            if (_fov == value)
                return;

            _fov = value;
            MarkProjectionMatrixDirty();
        }
    }

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
                float viewportWidth = RenderSystem.Instance.FramebufferSize.X * _viewRect.Size.X;
                float viewportHeight = RenderSystem.Instance.FramebufferSize.Y * _viewRect.Size.Y;

                float aspect = viewportWidth / viewportHeight;

                switch(ProjectionMode)
                {
                    case ProjectionMode.Orthogonal:
                        float height = _size * 2.0f;
                        float width = height * aspect;
                        _cachedProjMatrix = Matrix4x4.CreateOrthographic(width, height, _near, _far);
                        break;
                    case ProjectionMode.Perspective:
                        float fovRadians = _fov * (MathF.PI / 180.0f);
                        _cachedProjMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspect, _near, _far);
                        break;
                }
                _projMatrixDirty = false;
            }

            return _cachedProjMatrix;
        }
    }

    private ProjectionMode _projectionMode = ProjectionMode.Orthogonal;
    private Rectangle<float> _viewRect = new(0, 0, 1, 1);
    private float _near = -10.0f;
    private float _far = 10.0f;
    private float _size = 5.0f;
    private float _fov = 60.0f;
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
        RenderSystem.Instance.UnregisterCamera(this);
    }

    private void HandleTransformMatrixChange(Transform t) => MarkViewMatrixDirty();

    internal void MarkViewMatrixDirty() => _viewMatrixDirty = true;

    internal void MarkProjectionMatrixDirty() => _projMatrixDirty = true;

    /// <summary>
    /// 월드 좌표를 화면(픽셀) 좌표로 변환합니다. 카메라 뒤쪽(화면에 표시 불가)이면 null을 반환합니다.
    /// </summary>
    public Vector2? WorldToScreenPoint(Vector3 worldPos)
    {
        var clipPos = Vector4.Transform(new Vector4(worldPos, 1f), ViewMatrix * ProjectionMatrix);

        // 카메라 뒤쪽에 있으면 화면에 표시 불가
        if (clipPos.W <= 0f)
            return null;

        // NDC로 정규화 (-1 ~ 1)
        float ndcX = clipPos.X / clipPos.W;
        float ndcY = clipPos.Y / clipPos.W;

        // 이 카메라가 실제로 그려지는 뷰포트 영역 (프레임버퍼 기준 픽셀)
        var window = WindowControl.Size;

        float viewportX = window.X * ViewRect.Origin.X;
        float viewportY = window.Y * ViewRect.Origin.Y;
        float viewportWidth = window.X * ViewRect.Size.X;
        float viewportHeight = window.Y * ViewRect.Size.Y;

        float screenX =
            viewportX +
            (ndcX * 0.5f + 0.5f) * viewportWidth;

        float screenY =
            (window.Y - viewportY - viewportHeight) +
            (1f - (ndcY * 0.5f + 0.5f)) * viewportHeight;

        return new Vector2(screenX, screenY);
    }
}