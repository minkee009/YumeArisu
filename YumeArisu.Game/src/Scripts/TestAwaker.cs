using Silk.NET.Input;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Hierarchy;
using System.Collections;

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

        if (Input.GetKeyDown(Key.M))
        {
            StartCoroutine(Attack());
        }
    }

    public IEnumerator Attack()
    {
        System.Console.WriteLine("오옷");
        yield return StartCoroutine(Thinking());
        yield return new WaitForSeconds(0.725f);
        System.Console.Write("!");
        yield return new WaitForSeconds(1.5f);
        System.Console.WriteLine("끝남");
    }

    public IEnumerator Thinking()
    {
        var wait = new WaitForSeconds(0.1f);

        yield return wait;
        System.Console.Write(".");
        yield return wait;
        System.Console.Write(".");
        yield return wait;
        System.Console.Write(".");
        yield return wait;
        System.Console.Write(".");
        yield return wait;
        System.Console.Write(".");
        yield return wait;
        System.Console.Write(".");
        yield return wait;
        System.Console.Write(".");
    }
}