using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Game.Scenes;

namespace YumeArisu.Game;

public class GameSceneBootstrap : ISceneBootstrap
{
    public Scene Entry => new TestScene1();
    public Scene[] DynamicScenes =>  [new TestScene2(), new TestScene3()];
    public Scene[] StaticScenes => [new StaticScene()];
}