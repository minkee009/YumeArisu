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
        go.Transform = go.AddComponent<Transform>();
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

    public GameObject Find(string name)
    {
        foreach (var go in _gameObjects)
        {
            if (!go.ActiveInHierarchy || go.IsDestroyed)
                continue;
            if (go.Name == name)
                return go;
        }
        return null;
    }

    public GameObject FindWithTag(string tag) 
    { 
        foreach (var go in _gameObjects)
        {
            if (!go.ActiveInHierarchy || go.IsDestroyed)
                continue;
            if (go.Tag == tag)
                return go;
        }
        return null;
    }

    public List<GameObject> FindGameObjectsWithTag(string tag) 
    { 
        List<GameObject> matches = new();
        foreach (var go in _gameObjects)
        {
            if (!go.ActiveInHierarchy || go.IsDestroyed)
                continue;
            if (go.Tag == tag)
                matches.Add(go);
        }
        return matches;
    }

    public T FindObjectOfType<T>() where T : Component 
    { 
        foreach (var go in _gameObjects)
        {
            if (!go.ActiveInHierarchy || go.IsDestroyed)
                continue;

            var comp = go.GetComponent<T>();
            if(comp != null)
                return comp;
        }

        return null;
    }

    public List<T> FindObjectsOfType<T>() where T : Component
    {
        List<T> matches = new();
        foreach (var go in _gameObjects)
        {
            if (!go.ActiveInHierarchy || go.IsDestroyed)
                continue;

            matches.AddRange(go.GetComponents<T>());
        }
        return matches;
    }
}
