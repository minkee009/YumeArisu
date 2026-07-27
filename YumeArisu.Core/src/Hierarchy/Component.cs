namespace YumeArisu.Core.Hierarchy;

public abstract class Component
{
    public GameObject GameObject { get; internal set; }
    protected internal virtual void OnAttached() { }
    protected internal virtual void OnDetached() { }

    /// <summary>
    /// 자신을 소유한 게임오브젝트에게 자신을 제거하는 요청을 보냅니다.
    /// </summary>
    public void RemoveSelf() => GameObject.RemoveComponent(this);
}