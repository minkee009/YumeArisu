using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using Silk.NET.Input;
using YumeArisu.Core.Hierarchy;

public class TestAwaker : ScriptBehaviour
{
    public GameObject Target;
    public override void Update()
    {
        if(Input.GetKeyDown(Key.G) && Target != null)
        {
            System.Console.WriteLine($"타겟을 깨웁니다!!! -> {Target.Name}");
            Target.ActiveSelf = true;
        }
    }
}