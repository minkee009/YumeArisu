using System.Numerics;
using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Hierarchy;

public class Transform : Component
{
    public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
    public float Scale { get; set; } = 1f;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Transform Parent { get; private set; }
    public ReadOnlyListView<Transform> Children => _children;
    public int ChildCount => _children.Count;

    internal List<Transform> _children = new();

    public void SetParent(Transform parent)
    {
        if (parent == this)
            throw new InvalidOperationException("자기 자신을 부모로 설정할 수 없습니다.");

        Transform current = parent;
        while (current != null)
        {
            if (current == this)
                throw new InvalidOperationException("자신의 하위 계층(자식/손자 등)을 부모로 설정할 수 없습니다.");
            current = current.Parent;
        }

        Parent?.RemoveChild(this);
        parent?.AddChild(this);
        Parent = parent;
        GameObject?.RefreshActiveInHierarchy();
    }

    public Transform GetChild(int index) => _children[index];
    private void AddChild(Transform child) => _children.Add(child);
    private void RemoveChild(Transform child) => _children.Remove(child);
}