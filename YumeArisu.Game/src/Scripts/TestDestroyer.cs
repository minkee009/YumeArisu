using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using Silk.NET.Input;
using YumeArisu.Core.Hierarchy;

public class TestDestroyer : ScriptBehaviour
{
    public override void Awake()
    {
        System.Console.WriteLine("잠에서 일어남");
    }

    public override void Start()
    {
        System.Console.WriteLine("시작할게요!");
    }

    public override void OnEnable()
    {
        System.Console.WriteLine("켜졌어요");    
    }

    public override void Update()
    {
        if(Input.GetKeyDown(Key.F))
        {
            GameObject.Destroy();
        }
    }

    public override void OnDisable()
    {
        System.Console.WriteLine("꺼졌슴다!");    
    }

    public override void OnDestroy()
    {
        System.Console.WriteLine("파괴됨....");
    }
}