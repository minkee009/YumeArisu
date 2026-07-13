using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Game.Scenes;

public class TestScene2 : Scene
{
    public override void Load()
    {
        Console.WriteLine("ㅎㅎ ㅋㅋ ㅈㅅ");
        GameObject go = CreateGameObject("히후미");
        GameObject go1 = CreateGameObject("센세");
        GameObject go2 = CreateGameObject("히후미2");
        GameObject go3 = CreateGameObject("나기사");
        GameObject go4 = CreateGameObject("화난 히후미");
        GameObject go5 = CreateGameObject("냐기냐기");
        go.AddComponent<TestChatter>();
        go1.AddComponent<TestChatter>().Dialogue = 1;
        go2.AddComponent<TestChatter>().Dialogue = 2;
        go3.AddComponent<TestChatter>().Dialogue = 3;
        go4.AddComponent<TestChatter>().Dialogue = 4;
        go5.AddComponent<TestChatter>().Dialogue = 5;
        go3.AddComponent<TestDestroyer>();
    }
}