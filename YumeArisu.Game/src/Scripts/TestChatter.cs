using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using Silk.NET.Input;

public class TestChatter : ScriptBehaviour
{
    public int Dialogue { get; set; } = 0;
    public override void Update()
    {
        base.Update();
        if(Input.GetKeyDown(Key.Space))
        {
            switch(Dialogue)
            {
                case 1:
                    System.Console.WriteLine("히후미 네 이녀석");
                    break;
                case 2:
                    System.Console.WriteLine("(대충 다음 부터 그러지마세요 콘)");
                    break;
                case 3: 
                    System.Console.WriteLine("(친구비 내는 나기사 콘)");
                    break;
                case 4:
                    System.Console.WriteLine("(빠따콘)");
                    break;
                case 5:
                    System.Console.WriteLine("냐기 냐기 ~ 🐱");
                    break;
                default:
                    System.Console.WriteLine("아하하하하 그동안 즐거웠어요, 나기사님과 함께한 우정놀이");
                    break;
            }
            
        }
            
    }
}