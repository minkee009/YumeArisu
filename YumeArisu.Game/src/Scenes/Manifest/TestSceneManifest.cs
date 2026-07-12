using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Game.Scenes.Manifset;

public class TestSceneManifest : ISceneManifest
{
    public Scene[] DynamicScenes =>  [new TestScene1(), new TestScene2(), new TestScene3()];
    public Scene[] StaticScenes => [new StaticScene()];
}