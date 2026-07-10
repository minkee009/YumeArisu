using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Game.Scenes;

namespace YumeArisu.Game;

public class GameSceneBootstrap : ISceneBootstrap
{
    public Scene Entry => new TestScene1();
    public Scene[] CompiledScenes => [new TestScene2(), new StaticScene()];
}