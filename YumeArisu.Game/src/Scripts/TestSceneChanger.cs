using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using Silk.NET.Input;

namespace YumeArisu.Game.Scripts;

public class TestSceneChanger : ScriptBehaviour
{
    public Key DefaultKey = Key.R;
    public override void OnUpdate()
    {
        if (Input.GetKeyUp(DefaultKey))
        {
            SceneControl.ChangeScene("TestScene3");
        }
    }
}