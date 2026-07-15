using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using Silk.NET.Input;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Utility;

public class TestDestroyer : ScriptBehaviour
{
    public Behaviour OnEnableTarget; // 죽으면서 켤 Behaviour 컴포넌트 대상
    string _이가;
    string _은는;
    string _을를;
    public override void Awake()
    {
        _이가 = KoreanPostposition.GetIGa(GameObject.Name);
        _은는 = KoreanPostposition.GetEunNeun(GameObject.Name);
        _을를 = KoreanPostposition.GetEulReul(GameObject.Name);
        System.Console.WriteLine($"{GameObject.Name}{_이가} 잠에서 일어남");
    }

    public override void Start()
    {
        System.Console.WriteLine($"{GameObject.Name}{_은는} 말했다 : 시작할게요!");
    }

    public override void OnEnable()
    {
        System.Console.WriteLine($"{GameObject.Name}{_은는} 켜졌어요");    
    }

    public override void Update()
    {
        if (Input.GetKeyDown(Key.F))
        {
            GameObject.Destroy();
        }
    }

    public override void OnDisable()
    {
        System.Console.WriteLine($"{GameObject.Name}{_은는} 꺼졌슴다!");    
    }

    public override void OnDestroy()
    {
        System.Console.WriteLine($"{GameObject.Name}{_이가} 파괴됨....");
        if (OnEnableTarget != null)
        {
            System.Console.WriteLine("켤 대상이 있음 -> 접근함, 키겠음....");
            OnEnableTarget.Enabled = true;
            System.Console.WriteLine($"켰음 -> 대상 : {OnEnableTarget.GameObject.Name}");
        }
    }
}