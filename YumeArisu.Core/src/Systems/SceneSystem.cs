using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Core.Systems;

public class SceneSystem : SystemBase<SceneSystem, ISceneManifest>
{
    public Scene CurrentScene => _currentScene;

    private Dictionary<string,Scene> _dynamicScenes;
    private List<Scene> _staticScenes;
    private Scene _currentScene;
    private Scene _nextScene;

    internal override void StartUpInternal(ISceneManifest bootstrap)
    {
        _nextScene = bootstrap.DynamicScenes.FirstOrDefault();

        if (_nextScene == null)
            throw new InvalidOperationException("진입용 Scene이 존재하지 않습니다!");        

        _dynamicScenes = new();
        _staticScenes = new();

        foreach(var scene in bootstrap.DynamicScenes)
        {
            string name = scene.GetType().Name;
            _dynamicScenes.Add(name,scene);
        }

        foreach(var scene in bootstrap.StaticScenes)
        {
            _staticScenes.Add(scene);
            scene.Load(); // 검증 필요 -> 불안정 시 플래그를 만들고 Update()의 맨상단에서 최초 1회 load 순회를 돌아야 함
        }
    }

    internal override void ShutDownInternal()
    {
        _currentScene.Unload();
        foreach(var scene in _staticScenes) 
            scene.Unload();

        _dynamicScenes = null;
        _staticScenes = null;
        _currentScene = null;
        _nextScene = null;
    }

    public void ChangeScene(string sceneName)
    {
        if(_dynamicScenes.TryGetValue(sceneName,out Scene foundScene))
        {
            _nextScene = foundScene;
            return;
        }

        throw new InvalidOperationException("유효하지 않은 씬 이름입니다!.");
    }

    public void ChangeScene<T>() where T : Scene
    {
        ChangeScene(typeof(T).Name);
    }

    public void Update()
    {
        if(_nextScene == null)
            return;

        _currentScene?.Unload();
        
        _currentScene = _nextScene;
        _nextScene = null;

        _currentScene?.Load();
    }

    public GameObject Find(string name)
    {
        var target = _currentScene?.Find(name);
        if (target != null) 
            return target;

        foreach (var scene in _staticScenes)
        {
            target = scene.Find(name);
            if (target != null)
                return target;
        }
        return null;
    }

    public GameObject FindWithTag(string tag)
    {
        var target = _currentScene?.FindWithTag(tag);
        if (target != null) 
            return target;

        foreach (var scene in _staticScenes)
        {
            target = scene.FindWithTag(tag);
            if (target != null)
                return target;
        }
        return null;
    }

    public List<GameObject> FindGameObjectsWithTag(string tag)
    {
        List<GameObject> matches = new();

        if (_currentScene != null)
            matches.AddRange(_currentScene.FindGameObjectsWithTag(tag));

        foreach (var scene in _staticScenes)
            matches.AddRange(scene.FindGameObjectsWithTag(tag));

        return matches;
    }

    public T FindObjectOfType<T>() where T : Component
    {
        var target = _currentScene?.FindObjectOfType<T>();
        if (target != null)
            return target;

        foreach (var scene in _staticScenes)
        {
            target = scene.FindObjectOfType<T>();
            if (target != null)
                return target;
        }
        return null;
    }

    public List<T> FindObjectsOfType<T>() where T : Component
    {
        List<T> matches = new();

        if (_currentScene != null)
            matches.AddRange(_currentScene.FindObjectsOfType<T>());

        foreach (var scene in _staticScenes)
            matches.AddRange(scene.FindObjectsOfType<T>());

        return matches;
    }
}

// 문법 설탕용 클래스 (씬 변경)
public static class SceneControl
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ChangeScene(string sceneName) => SceneSystem.Instance.ChangeScene(sceneName);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ChangeScene<T>() where T : Scene => SceneSystem.Instance.ChangeScene<T>();
}

// 문법 설탕용 클래스 (오브젝트/컴포넌트 검색)
public static class SceneQuery
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GameObject Find(string name) => SceneSystem.Instance.Find(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GameObject FindWithTag(string tag) => SceneSystem.Instance.FindWithTag(tag);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<GameObject> FindGameObjectsWithTag(string tag) => SceneSystem.Instance.FindGameObjectsWithTag(tag);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T FindObjectOfType<T>() where T : Component => SceneSystem.Instance.FindObjectOfType<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> FindObjectsOfType<T>() where T : Component => SceneSystem.Instance.FindObjectsOfType<T>();
}