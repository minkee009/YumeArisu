using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Game.Scenes;

namespace YumeArisu.Game.SceneManifests;

public class TestSceneManifest : ISceneManifest
{
    public Scene[] DynamicScenes =>  [new TestScene1(), new TestScene2(), new EmptyScene()];
    public Scene StaticScene => new StaticScene();
}