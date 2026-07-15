using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using Silk.NET.Input;

public class TestSceneChanger : ScriptBehaviour
{
    public Key DefaultKey = Key.R;
    public override void Update()
    {
        if(Input.GetKeyUp(DefaultKey))
        {
            SceneControl.ChangeScene("EmptyScene");
        }
    }
}