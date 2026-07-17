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

    // Find API, 출시 전 까지 쓸 일 없으면 Deprecate
    // public Transform FindChild(string path)
    // {
    //     if (string.IsNullOrEmpty(path))
    //         return null;

    //     ReadOnlySpan<char> remaining = path;
    //     Transform current = this;

    //     while (!remaining.IsEmpty)
    //     {
    //         int slash = remaining.IndexOf('/');
    //         ReadOnlySpan<char> segment = slash >= 0 ? remaining[..slash] : remaining;
    //         remaining = slash >= 0 ? remaining[(slash + 1)..] : default;

    //         if (segment.IsEmpty)
    //             continue; // 연속 슬래시("A//B") 방어

    //         current = FindImmediateChild(current, segment);
    //         if (current == null)
    //             return null;
    //     }

    //     return current;
    // }

    // private static Transform FindImmediateChild(Transform parent, ReadOnlySpan<char> name)
    // {
    //     for (int i = 0; i < parent.ChildCount; i++)
    //     {
    //         var child = parent.GetChild(i);
    //         if (name.SequenceEqual(child.GameObject.Name))
    //             return child;
    //     }
    //     return null;
    // }
}