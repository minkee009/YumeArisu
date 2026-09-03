using System.Numerics;
using YumeArisu.Core.Common;

namespace YumeArisu.Core.Hierarchy;

public class Transform : Component
{
    public Vector3 LocalPosition 
    { 
        get => _localPosition; 
        set
        {
            _localPosition = value;
            MarkLocalMatrixDirty();
        } 
    }

    public Quaternion LocalRotation 
    {
        get => _localRotation; 
        set
        {
            _localRotation = value;
            MarkLocalMatrixDirty();
        } 
    }

    public Vector3 LocalScale 
    { 
        get => _localScale; 
        set
        {
            _localScale = value;
            MarkLocalMatrixDirty();
        } 
    }

    public Vector3 WorldPosition 
    { 
        get => WorldMatrix.Translation;
        set
        {
            if (Parent is not null)
            {
                Matrix4x4.Invert(Parent.WorldMatrix, out var inverse);
                LocalPosition = Vector3.Transform(value, inverse);
            }
            else
            {
                LocalPosition = value;
            }
        }
    } 

    /// <summary>
    /// 축 정렬(non-sheared) 계층 구조를 가정한 근사 월드 스케일.
    /// 조상 중 회전 + 비균등 스케일이 섞여 있으면 set/get 결과가 정확히 일치하지 않을 수 있습니다.
    /// </summary>
    public Vector3 WorldLossyScale 
    { 
        get => LocalScale * (Parent?.WorldLossyScale ?? Vector3.One);
        set
        {
            if (Parent is not null)
            {
                var parentScale = Parent.WorldLossyScale;

                const float epsilon = 1e-6f;
                LocalScale = new Vector3(
                    (MathF.Abs(parentScale.X) > epsilon) ? value.X / parentScale.X : value.X,
                    (MathF.Abs(parentScale.Y) > epsilon) ? value.Y / parentScale.Y : value.Y,
                    (MathF.Abs(parentScale.Z) > epsilon) ? value.Z / parentScale.Z : value.Z);
            }
            else
            {
                LocalScale = value;
            }
        } 
    }

    public Quaternion WorldRotation 
    { 
        get => LocalRotation * (Parent?.WorldRotation ?? Quaternion.Identity); 
        set
        {
            if (Parent is not null)
            {
                var inverse = Quaternion.Inverse(Parent.WorldRotation);
                LocalRotation = inverse * value;
            }
            else
            {
                LocalRotation = value;
            }
        } 
    } 

    public Transform Parent { get; private set; }
    public ReadOnlyListView<Transform> Children => _children;
    public Action<Transform> WorldMatrixDirtyChange;
    public int ChildCount => _children.Count;

    public Matrix4x4 LocalMatrix
    {
        get
        {
            if (_localMatrixDirty)
            {
                _cachedLocalMatrix =
                    Matrix4x4.CreateScale(LocalScale)
                    * Matrix4x4.CreateFromQuaternion(LocalRotation)
                    * Matrix4x4.CreateTranslation(LocalPosition);
                _localMatrixDirty = false;
            }
            return _cachedLocalMatrix;
        }
    }

    public Matrix4x4 WorldMatrix
    {
        get
        {
            if (_worldMatrixDirty)
            {
                _cachedWorldMatrix = LocalMatrix * (Parent?.WorldMatrix ?? Matrix4x4.Identity);
                _worldMatrixDirty = false;
            }
            return _cachedWorldMatrix;
        }
    }

    internal List<Transform> _children = new();
    
    private Vector3 _localPosition = Vector3.Zero;
    private Quaternion _localRotation = Quaternion.Identity;
    private Vector3 _localScale = Vector3.One;

    private Matrix4x4 _cachedLocalMatrix;
    private Matrix4x4 _cachedWorldMatrix;
    private bool _localMatrixDirty = true;
    private bool _worldMatrixDirty = true;

    private void MarkLocalMatrixDirty()
    {
        _localMatrixDirty = true;
        MarkWorldMatrixDirty();
    }

    private void MarkWorldMatrixDirty()
    {
        // 이미 dirty라면 자식들도 이미 전파가 끝난 상태이므로 더 내려갈 필요 없음
        if (_worldMatrixDirty)
            return;

        _worldMatrixDirty = true;
        WorldMatrixDirtyChange?.Invoke(this);

        foreach (var child in _children)
            child.MarkWorldMatrixDirty();
    }

    public void SetParent(Transform parent, bool worldPositionStays = true)
    {
        if (parent == this)
            throw new InvalidOperationException("자기 자신을 부모로 설정할 수 없습니다.");

        Transform current = parent;
        while (current is not null)
        {
            if (current == this)
                throw new InvalidOperationException("자신의 하위 계층(자식/손자 등)을 부모로 설정할 수 없습니다.");
            current = current.Parent;
        }

        var worldPositionBefore = WorldPosition;
        var worldRotationBefore = WorldRotation;
        var worldLossyScaleBefore = WorldLossyScale;

        Parent?.RemoveChild(this);
        parent?.AddChild(this);
        Parent = parent;
        MarkWorldMatrixDirty(); // 부모 참조 자체가 바뀌었으므로 Local이 그대로여도 World는 무효화 필요
        GameObject?.RefreshActiveInHierarchy();

        if (worldPositionStays)
        {
            if (Parent is not null)
            {
                // 부모 기준 상 로컬 행렬
                var parentPos = parent.WorldPosition;
                var parentRot = parent.WorldRotation;
                var parentScale = parent.WorldLossyScale;

                var inverseRot = Quaternion.Inverse(parentRot);

                const float eps = 1e-6f;

                // position
                Vector3 stayPosition = worldPositionBefore - parentPos;
                stayPosition = Vector3.Transform(stayPosition, inverseRot);

                LocalPosition = new Vector3(
                    (MathF.Abs(parentScale.X) > eps) ? stayPosition.X / parentScale.X : stayPosition.X,
                    (MathF.Abs(parentScale.Y) > eps) ? stayPosition.Y / parentScale.Y : stayPosition.Y,
                    (MathF.Abs(parentScale.Z) > eps) ? stayPosition.Z / parentScale.Z : stayPosition.Z);

                // scale
    
                LocalScale = new Vector3(
                    (MathF.Abs(parentScale.X) > eps) ? worldLossyScaleBefore.X / parentScale.X : worldLossyScaleBefore.X,
                    (MathF.Abs(parentScale.Y) > eps) ? worldLossyScaleBefore.Y / parentScale.Y : worldLossyScaleBefore.Y,
                    (MathF.Abs(parentScale.Z) > eps) ? worldLossyScaleBefore.Z / parentScale.Z : worldLossyScaleBefore.Z);

                // rotation
                LocalRotation = inverseRot * worldRotationBefore;
                LocalRotation = Quaternion.Normalize(LocalRotation);

                
            }
            else
            {
                // 부모가 없으면 로컬 = 월드
                LocalPosition = worldPositionBefore;
                LocalRotation = worldRotationBefore;
                LocalScale = worldLossyScaleBefore;
            }  
        }
    }

    public Transform GetChild(int index) => _children[index];
    private void AddChild(Transform child) => _children.Add(child);
    private void RemoveChild(Transform child) => _children.Remove(child);
}