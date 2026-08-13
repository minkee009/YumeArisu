using System.Runtime.CompilerServices;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Core.Systems;

public class SceneSystem : SystemBase<SceneSystem, ISceneManifest>
{
    public Scene CurrentScene => _currentScene;
    public Scene StaticScene => _staticScene;

    public event Action BeforeSceneChange;
    public event Action AfterSceneChange;

    private Dictionary<string, Scene> _dynamicScenes;
    private Scene _staticScene;
    private Scene _currentScene;
    private Scene _nextScene;

    internal override void OnStartUp(ISceneManifest manifest)
    {
        _nextScene = manifest.DynamicScenes.FirstOrDefault();

        if (_nextScene is null)
            throw new InvalidOperationException("진입용 Scene이 존재하지 않습니다!");        

        _dynamicScenes = new();

        foreach (var scene in manifest.DynamicScenes)
        {
            string name = scene.GetType().Name;
            _dynamicScenes.Add(name,scene);
        }

        _staticScene = manifest.StaticScene;
    }

    internal override void OnShutDown()
    {
        _currentScene?.Unload();
        _staticScene?.Unload();

        BeforeSceneChange = null;
        AfterSceneChange = null;

        _dynamicScenes = null;
        _staticScene = null;
        _currentScene = null;
        _nextScene = null;
    }

    public void ChangeScene(string sceneName)
    {
        if (_dynamicScenes.TryGetValue(sceneName,out Scene foundScene))
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
        if (!_staticScene?.IsLoaded ?? false)
            _staticScene.Load();

        if (_nextScene is null)
            return;

        _currentScene?.Unload();
        
        _currentScene = _nextScene;
        _nextScene = null;

        BeforeSceneChange?.Invoke();

        GC.Collect(2, GCCollectionMode.Optimized);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Optimized); // finalizer가 새로 만든 쓰레기 정리

        _currentScene?.Load();

        AfterSceneChange?.Invoke();

        return;
    }

    public GameObject FindAnyGameObject(string name)
    {
        var target = _currentScene?.FindGameObject(name);
        if (target is not null) 
            return target;

        target = _staticScene?.FindGameObject(name);
        if (target is not null)
            return target;
        return null;
    }

    public GameObject FindAnyGameObjectByTag(string tag)
    {
        var target = _currentScene?.FindGameObjectByTag(tag);
        if (target is not null) 
            return target;

        target = _staticScene?.FindGameObjectByTag(tag);
        if (target is not null)
            return target;

        return null;
    }

    public List<GameObject> FindAnyGameObjectsByTag(string tag)
    {
        List<GameObject> matches = new();

        if (_currentScene is not null)
            matches.AddRange(_currentScene.FindGameObjectsByTag(tag));

        if (_staticScene is not null)
            matches.AddRange(_staticScene.FindGameObjectsByTag(tag));

        return matches;
    }

    public GameObject FindAnyGameObjectByLayer(uint layer)
    {
        var target = _currentScene?.FindGameObjectByLayer(layer);
        if (target is not null) 
            return target;

        target = _staticScene?.FindGameObjectByLayer(layer);
        if (target is not null)
            return target;

        return null;
    }

    public List<GameObject> FindAnyGameObjectsByLayer(uint layer)
    {
        List<GameObject> matches = new();

        if (_currentScene is not null)
            matches.AddRange(_currentScene.FindGameObjectsByLayer(layer));

        if (_staticScene is not null)
            matches.AddRange(_staticScene.FindGameObjectsByLayer(layer));

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
        public static GameObject FindGameObjectByTag(string tag) => SceneSystem.Instance.FindAnyGameObjectByTag(tag);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsByTag(string tag) => SceneSystem.Instance.FindAnyGameObjectsByTag(tag);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObjectByLayer(uint layer) => SceneSystem.Instance.FindAnyGameObjectByLayer(layer);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsByTag(uint layer) => SceneSystem.Instance.FindAnyGameObjectsByLayer(layer);
    }

    public static class Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObject(string name) => SceneSystem.Instance.CurrentScene?.FindGameObject(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObjectByTag(string tag) => SceneSystem.Instance.CurrentScene?.FindGameObjectByTag(tag);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsByTag(string tag) => SceneSystem.Instance.CurrentScene?.FindGameObjectsByTag(tag);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObjectByLayer(uint layer) => SceneSystem.Instance.CurrentScene?.FindGameObjectByLayer(layer);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsByTag(uint layer) => SceneSystem.Instance.CurrentScene?.FindGameObjectsByLayer(layer);
    }

    public static class Static
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObject(string name) => SceneSystem.Instance.StaticScene?.FindGameObject(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObjectByTag(string tag) => SceneSystem.Instance.StaticScene?.FindGameObjectByTag(tag);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsByTag(string tag) => SceneSystem.Instance.StaticScene?.FindGameObjectsByTag(tag);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindGameObjectByLayer(uint layer) => SceneSystem.Instance.StaticScene?.FindGameObjectByLayer(layer);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<GameObject> FindGameObjectsByTag(uint layer) => SceneSystem.Instance.StaticScene?.FindGameObjectsByLayer(layer);
    }
}