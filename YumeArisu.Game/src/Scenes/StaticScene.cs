using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;

namespace YumeArisu.Game.Scenes;

public class StaticScene : Scene
{
    public GameObject TestManager;

    protected override void OnLoad()
    {
        System.Console.WriteLine("프로그램이 종료될 때까지 유지되는 씬입니다.");
        TestManager = CreateGameObject("테스트 매니저");
    }
}