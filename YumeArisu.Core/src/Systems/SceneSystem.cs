using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Core.Systems;

public class SceneSystem : SystemBase<SceneSystem, Scene[]>
{
    private Dictionary<string,Scene> _scenes;
    private Scene _currentScene;
    private Scene _nextScene;
    public override void StartUpInternal(Scene[] playlist)
    {
        _nextScene = playlist.FirstOrDefault();
        if (_nextScene == null)
        {
            throw new InvalidOperationException("Scene 플레이 리스트가 존재하지 않습니다!");
        }

        _scenes = new();
        foreach(var scene in playlist)
        {
            string name = scene.GetType().Name;
            _scenes.Add(name,scene);
        }
    }

    public override void ShutDownInternal()
    {
        _scenes = null;
        _currentScene = null;
        _nextScene = null;
    }

    public void ChangeScene(string sceneName)
    {
        if(_scenes.TryGetValue(sceneName,out Scene foundScene))
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

        if(_currentScene != null)
            _currentScene.Unload();
        
        _currentScene = _nextScene;
        _nextScene = null;

        _currentScene.Load();
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