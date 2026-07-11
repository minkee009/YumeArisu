using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Core.Systems;

public class SceneSystem : SystemBase<SceneSystem, ISceneBootstrap>
{
    public Scene CurrentScene => _currentScene;

    private Dictionary<string,Scene> _dynamicScenes;
    private List<Scene> _staticScenes;
    private Scene _currentScene;
    private Scene _nextScene;

    internal override void StartUpInternal(ISceneBootstrap bootstrap)
    {
        _nextScene = bootstrap.Entry;

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
}

// 문법 설탕용 클래스
public static class SceneControl
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ChangeScene(string sceneName) => SceneSystem.Instance.ChangeScene(sceneName);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ChangeScene<T>() where T : Scene => SceneSystem.Instance.ChangeScene<T>();
}