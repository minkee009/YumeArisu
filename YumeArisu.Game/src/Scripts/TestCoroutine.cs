using Silk.NET.Input;
using System.Collections;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Routines;
using System.Reflection.Metadata;

public class TestCoroutine : ScriptBehaviour
{
    public override void Start()
    {
        
    }
    
    public override void Update()
    {
        if (Input.GetKeyDown(Key.E))
        {
            StartCoroutine(RunCode());
        }

        if (Input.GetKeyDown(Key.M))
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator RunCode()
    {
        string text = "「무량공처」 무하한의 안쪽인 이 영역은 고죠 사토루 본인을 제외하고 모든 대상이 행하는 정신 활동을 무한한 반복작업으로 만듬. 영역에 잠시라도 발을 들이는 순간 뇌가 블루스크린 상태가 되어 아무 것도 할 수 없으며, 이 상태가 조금만 길게 이어져도 영구적인 뇌 손상으로 폐인이 되어 버림.";
        for (int i = 0; i < text.Length; i++)
        {
            System.Console.Write(text[i]);
            yield return null; yield return null; yield return null;
            yield return null; yield return null; yield return null;
            yield return null; yield return null; yield return null;
            yield return null; yield return null; yield return null;
            yield return null; yield return null; yield return null;
            yield return null; yield return null; yield return null;
            yield return null; yield return null; yield return null;
            yield return null; yield return null; yield return null;
            yield return null; yield return null; yield return null;
        } 
        System.Console.Write("\n");
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

        for (int i = 0; i < 7; i++)
        {
            yield return wait;
            System.Console.Write(".");
        }
    }
}