using System.Numerics;
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
        camera.Transform.LocalPosition = new(0, 0, 10.0f);
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
        yugo.AddComponent<Rotater>().RotateSpeed = 25.0f;

        GameObject yugo2 = CreateGameObject();
        yugo2.Transform.LocalPosition = Vector3.Zero;
        var renderer2 = yugo2.AddComponent<SpriteRenderer>();
        renderer2.Sprite = yuukaSpr;
        renderer2.Enabled = true;
        renderer2.Color = Color.Cyan;
        yugo2.AddComponent<Rotater>().RotateSpeed = 45.0f;
        yugo2.Transform.SetParent(yugo.Transform, false);
        yugo2.Transform.LocalPosition += new Vector3(12.0f,0f,0f);
        yugo2.Transform.LocalScale = new Vector3(0.5f, 0.5f, 0.5f);

        GameObject yugo3 = CreateGameObject();
        yugo3.Transform.LocalPosition =  new(0,0,5.0f);
        var renderer3 = yugo3.AddComponent<SpriteRenderer>();
        renderer3.Sprite = yuukaSpr;
        renderer3.Enabled = true;
        renderer3.Color = Color.Magenta;
        yugo3.AddComponent<Rotater>().RotateSpeed = 60.0f;
        yugo3.Transform.SetParent(yugo2.Transform, false);
        yugo3.Transform.LocalPosition += new Vector3(8.0f,0f,0f);
        yugo3.Transform.LocalScale = new Vector3(0.25f, 0.25f, 0.25f);


        GameObject yugo4 = CreateGameObject();
        yugo4.Transform.LocalPosition =  new(0,-24.0f,0.0f);
        yugo4.Transform.LocalRotation = Quaternion.CreateFromAxisAngle(new(1.0f,0.0f,0.0f), 90.0f /  57.2957795f);
        yugo4.Transform.LocalScale = new(12,12,1);

        var renderer4 = yugo4.AddComponent<SpriteRenderer>();
        renderer4.Sprite = yuukaSpr;
        renderer4.Enabled = true;
        renderer4.Color = Color.Yellow;
        renderer4.FlipY = true;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Resources.Release(yuukaTex);
    }
}