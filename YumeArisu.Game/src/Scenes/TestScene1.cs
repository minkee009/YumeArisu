using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;

namespace YumeArisu.Game.Scenes;

public class TestScene1 : Scene
{
    public override bool IsStatic => false;
    private GameObject _player;
    public override void Load()
    {
        _player = CreateGameObject("Player");
        System.Console.WriteLine("크하하 플레이어가 생성됐다고!!"); // 센포티스런 대사

        System.Console.WriteLine("죄송하지만 아가씨 다음 장면으로 넘어가시겠습니다..");
        SceneControl.ChangeScene("TestScene2");
    }
}