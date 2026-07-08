namespace YumeArisu.Core.Hierarchy;

public abstract class Component
{
    public required GameObject GameObject { get; init; }
    protected internal virtual void OnAttached() { }
    protected internal virtual void OnDetached() { }
}