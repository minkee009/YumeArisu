using System.Runtime.CompilerServices;
using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Hierarchy;

public abstract class Scene
{
    public ReadOnlyListView<GameObject> GameObjects => _gameObjects;
    
    public bool IsLoaded { get; private set; }

    internal List<GameObject> _gameObjects = new();

    internal void Load()
    {
        if (IsLoaded)
        {
            ConsoleExtensions.WriteLineColored("씬이 이미 로드되어 있습니다. Unload이후 다시 호출해주세요", ConsoleColor.Yellow);
            return;
        }

        OnLoad();

        IsLoaded = true;
    }

    internal void Unload()
    {
        if (!IsLoaded)
        {
            ConsoleExtensions.WriteLineColored("씬이 로드되어 있지 않습니다. Load 이후 호출해주세요", ConsoleColor.Yellow);
            return;
        }

        OnUnload();

        foreach (var go in _gameObjects)
            go.Destroy();
        _gameObjects.Clear();

        IsLoaded = false;
    }

    protected abstract void OnLoad();
    protected virtual void OnUnload() { }

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
        go.Destroy();
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

    public GameObject FindGameObjectByTag(string tag) 
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

    public List<GameObject> FindGameObjectsByTag(string tag) 
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

    public GameObject FindGameObjectByLayer(int layer)
    {
        foreach (var go in _gameObjects)
        {
            if (!go.ActiveInHierarchy || go.IsDestroyed)
                continue;
            if (go.Layer == layer)
                return go;
        }
        return null;
    }

    public List<GameObject> FindGameObjectsByLayer(int layer)
    {
        List<GameObject> matches = new();
        foreach (var go in _gameObjects)
        {
            if (!go.ActiveInHierarchy || go.IsDestroyed)
                continue;
            if (go.Layer == layer)
                matches.Add(go);
        }
        return matches;
    }
}
