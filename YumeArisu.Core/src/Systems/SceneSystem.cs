using System.Runtime.CompilerServices;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Core.Systems;

public class SceneSystem : SystemBase<SceneSystem, ISceneManifest>
{
    public Scene CurrentScene => _currentScene;
    public Scene StaticScene => _staticScene;

    private Dictionary<string,Scene> _dynamicScenes;
    private Scene _staticScene;
    private Scene _currentScene;
    private Scene _nextScene;

    internal override void StartUpInternal(ISceneManifest bootstrap)
    {
        _nextScene = bootstrap.DynamicScenes.FirstOrDefault();

        if (_nextScene == null)
            throw new InvalidOperationException("진입용 Scene이 존재하지 않습니다!");        

        _dynamicScenes = new();

        foreach(var scene in bootstrap.DynamicScenes)
        {
            string name = scene.GetType().Name;
            _dynamicScenes.Add(name,scene);
        }

        _staticScene = bootstrap.StaticScene;
        _staticScene?.Load();
    }

    internal override void ShutDownInternal()
    {
        _currentScene?.Unload();
        _staticScene?.Unload();

        _dynamicScenes = null;
        _staticScene = null;
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

    public void BeginFrame()
    {
        if(_nextScene == null)
            return;

        _currentScene?.Unload();
        
        _currentScene = _nextScene;
        _nextScene = null;

        GC.Collect(2, GCCollectionMode.Optimized);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Optimized); // finalizer가 새로 만든 쓰레기 정리

        _currentScene?.Load();
    }

    public GameObject FindAnyGameObject(string name)
    {
        var target = _currentScene?.FindGameObject(name);
        if (target != null) 
            return target;

        target = _staticScene?.FindGameObject(name);
        if (target != null)
            return target;
        return null;
    }

    public GameObject FindAnyGameObjectWithTag(string tag)
    {
        var target = _currentScene?.FindGameObjectWithTag(tag);
        if (target != null) 
            return target;

        target = _staticScene?.FindGameObjectWithTag(tag);
        if (target != null)
            return target;

        return null;
    }

    public List<GameObject> FindAnyGameObjectsWithTag(string tag)
    {
        List<GameObject> matches = new();

        if (_currentScene != null)
            matches.AddRange(_currentScene.FindGameObjectsWithTag(tag));

        if (_staticScene != null)
            matches.AddRange(_staticScene.FindGameObjectsWithTag(tag));

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

    public static Scene CurrentScene => SceneSystem.Instance.CurrentScene;
}

// 문법 설탕용 클래스 (쿼리별 게임 오브젝트 검색)
public static class SceneQuery
{
    public static class Global
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObject(string name) => SceneSystem.Instance.FindAnyGameObject(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObjectWithTag(string name) => SceneSystem.Instance.FindAnyGameObjectWithTag(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsWithTag(string name) => SceneSystem.Instance.FindAnyGameObjectsWithTag(name);
    }

    public static class Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObject(string name) => SceneSystem.Instance.CurrentScene?.FindGameObject(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObjectWithTag(string name) => SceneSystem.Instance.CurrentScene?.FindGameObjectWithTag(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsWithTag(string name) => SceneSystem.Instance.CurrentScene?.FindGameObjectsWithTag(name);
    }

    public static class Static
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObject(string name) => SceneSystem.Instance.StaticScene?.FindGameObject(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObjectWithTag(string name) => SceneSystem.Instance.StaticScene?.FindGameObjectWithTag(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsWithTag(string name) => SceneSystem.Instance.StaticScene?.FindGameObjectsWithTag(name);
    }
}