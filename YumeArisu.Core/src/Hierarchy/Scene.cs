namespace YumeArisu.Core.Hierarchy;

public abstract class Scene
{
    public IReadOnlyList<GameObject> GameObjects => _gameObjects;
    private List<GameObject> _gameObjects = new();
    public abstract void Load();
    public virtual void Unload()
    {
        foreach (var go in _gameObjects)
        {
            go.Destroy();
        }
        _gameObjects.Clear();
    }

    public GameObject CreateGameObject(string name = "",bool active = true)
    {
        GameObject go = new(this, name);
        _gameObjects.Add(go);
        go.ActiveSelf = active;
        return go;
    }

    public void DestroyGameObject(GameObject go)
    {
        ArgumentNullException.ThrowIfNull(go);
        _gameObjects.Remove(go);
        go.Destroy();
    }
}
