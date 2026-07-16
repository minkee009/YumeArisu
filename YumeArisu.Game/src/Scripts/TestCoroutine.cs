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
        if(Input.GetKeyDown(Key.E))
        {
            StartCoroutine(RunCode());
        }
    }

    IEnumerator RunCode()
    {
        for(int i = 0; i < 10; i++)
        {
            System.Console.WriteLine($"{i}프레임 지남");
            yield return null;
        }
    }
}