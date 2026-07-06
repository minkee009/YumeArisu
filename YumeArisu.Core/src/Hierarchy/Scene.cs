using Silk.NET.Vulkan;

namespace YumeArisu.Core.Hierarchy;

public abstract class Scene : IDisposable
{
    public IReadOnlyList<GameObject> GameObjects => _gameObjects;
    private List<GameObject> _gameObjects;
    public abstract void Initialize();
    public abstract void Dispose();
}
