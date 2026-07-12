namespace YumeArisu.Core.Hierarchy;

public abstract class Component
{
    public GameObject GameObject { get; internal set; }
    protected internal virtual void OnAttached() { }
    protected internal virtual void OnDetached() { }
}