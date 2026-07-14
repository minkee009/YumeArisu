using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using Silk.NET.Input;
using YumeArisu.Core.Hierarchy;

public class TestDestroyer : ScriptBehaviour
{
    public override void Update()
    {
        if(Input.GetKeyDown(Key.F))
        {
            GameObject.Destroy();
        }
    }
}