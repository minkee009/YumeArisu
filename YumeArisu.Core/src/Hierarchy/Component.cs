namespace YumeArisu.Core.Hierarchy;

public abstract class Component : IDisposable
{
    public required GameObject Owner { get; init; }

    public virtual void Dispose() { }
}