using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;

namespace YumeArisu.Game.Scenes;

public class TestScene1 : Scene
{
    private GameObject _player;
    private GameObject _weapon;
    public override void Load()
    {
        _player = CreateGameObject("Player");
        System.Console.WriteLine("크하하 플레이어가 생성됐다고!!"); // 센포티스런 대사

        _player.Transform.Position = new (30f,45f,0f);

        _weapon = CreateGameObject("Weapon");
        _weapon.Transform.SetParent(_player.Transform);

        System.Console.WriteLine("용사는 '무기'를 획득했다!");

        foreach(var tr in _player.Transform.Children)
            System.Console.WriteLine($"{tr.GameObject.Name} -> 플레이어 하위 객체");

        System.Console.WriteLine("죄송하지만 아가씨 다음 장면으로 넘어가시겠습니다..");
        SceneControl.ChangeScene("TestScene2");
    }

    public override void Unload()
    {
        base.Unload();
        System.Console.WriteLine($"{_player == null} -> 플레이어 null 상태");
    }
}