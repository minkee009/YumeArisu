using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Hierarchy;

public abstract class Scene
{
    public ReadOnlyListView<GameObject> GameObjects => _gameObjects;
    
    internal List<GameObject> _gameObjects = new();

    internal protected abstract void Load();

    internal protected virtual void Unload()
    {
        foreach (var go in _gameObjects)
            go.DestroyInternal();
        _gameObjects.Clear();
    }

    public GameObject CreateGameObject(string name = "",bool active = true)
    {
        GameObject go = new(this, name);
        go.Transform = go.AddComponent<Transform>();
        _gameObjects.Add(go);
        go.SetActive(active);
        return go;
    }

    public void DestroyGameObject(GameObject go)
    {
        ArgumentNullException.ThrowIfNull(go);
        
        DestroyRecursive(go);
    }

    private void DestroyRecursive(GameObject go)
    {
        if (go.IsDestroyed)
            return;

        // 먼저 자식들 제거 - 뒤에서 부터 순차적으로 제거 -> 순회할 때 자식이 하나씩 빠져도 인덱스 안정적
        for (int i = go.Transform.ChildCount - 1; i >= 0; i--)
            DestroyRecursive(go.Transform.GetChild(i).GameObject);

        // Scene 소유 리스트에서 제거
        _gameObjects.Remove(go);

        // GameObject 내부 정리
        go.DestroyInternal();
    }

    public GameObject FindGameObject(string name)
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

    public GameObject FindGameObjectWithTag(string tag) 
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
}
