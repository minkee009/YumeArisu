using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Systems;
using YumeArisu.Game.Scripts;

namespace YumeArisu.Game.Scenes;

public class TestScene2 : Scene
{
    Texture yuukaTex;
    Sprite yuukaSpr;
    protected override void OnLoad()
    {
        Console.WriteLine("ㅎㅎ ㅋㅋ ㅈㅅ");
        GameObject go = CreateGameObject("히후미");
        GameObject go1 = CreateGameObject("센세", false);
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

        go3.AddComponent<TestDestroyer>().Enabled = false;
        go3.AddComponent<TestAwaker>().Target = go1;

        go1.AddComponent<TestDestroyer>().OnEnableTarget = go3.GetComponent<TestDestroyer>();
        
        go2.AddComponent<TestSceneChanger>();

        GameObject childA = CreateGameObject("죄책감");
        GameObject childB = CreateGameObject("뻔뻔함");
        GameObject childC = CreateGameObject("의심");

        childA.Transform.SetParent(go3.Transform);
        childB.Transform.SetParent(go3.Transform);
        childC.Transform.SetParent(go3.Transform);

        go.AddComponent<FPSChecker>();
        go.AddComponent<TestCoroutine>();
        
        var camerago = CreateGameObject("MainCamera");
        var camera = camerago.AddComponent<Camera>();
        camerago.AddComponent<CamMover>();
        //go.AddComponent<WindowMover>();

        camera.Size = 5;
        camera.Transform.LocalPosition = new(0,0,0.0f);
        camera.FieldOfView = 60;
        camera.ProjectionMode = ProjectionMode.Perspective;
        camera.NearPlane = 0.01f;
        camera.FarPlane = 1000;


        yuukaTex = Resources.Get<Texture>("Yuuka.png");
        yuukaSpr = new Sprite();
        yuukaSpr.ImmediateLoadFromReference(yuukaTex, new(0.5f,0.5f),new(0,0,yuukaTex.Width,yuukaTex.Height));
        

        GameObject yugo = CreateGameObject();
        yugo.Transform.LocalPosition = new(0,0,-5.0f);
        var renderer = yugo.AddComponent<SpriteRenderer>();
        renderer.Sprite = yuukaSpr;
        renderer.Enabled = true;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Resources.Release(yuukaTex);
    }
}