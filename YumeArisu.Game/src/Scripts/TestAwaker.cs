using Silk.NET.Input;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Hierarchy;
using System.Collections;

namespace YumeArisu.Game.Scripts;

public class TestAwaker : ScriptBehaviour
{
    public GameObject Target { get; set; } = null;
    public override void Update()
    {
        if (Input.GetKeyDown(Key.G) && Target != null)
        {
            System.Console.WriteLine($"타겟을 깨웁니다!!! -> {Target.Name}");
            Target.SetActive(true);
            var destroyer = Target?.GetComponent<TestDestroyer>() ?? null;
            if (destroyer != null)
            {
                destroyer.Enabled = true;
            }
        }
    }
}