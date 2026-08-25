namespace YumeArisu.Core.Hierarchy;

public abstract class Component
{
    public GameObject GameObject { get; internal set; }

    /// 문법 설탕용 함수
    public Transform Transform => GameObject!.Transform;
    public T GetComponent<T>() where T : Component => GameObject!.GetComponent<T>();
    public T AddComponent<T>() where T : Component, new() => GameObject!.AddComponent<T>();
    /// --------------
    
    protected internal virtual void OnAttach() { }
    protected internal virtual void OnDetach() { }

    /// <summary>
    /// 자신을 소유한 게임오브젝트에게 자신을 제거하는 요청을 보냅니다.
    /// </summary>
    public void RemoveSelf() => GameObject.RemoveComponent(this);
}