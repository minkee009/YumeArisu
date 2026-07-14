using System.Collections;

namespace YumeArisu.Core.Routines;

public class ScriptBehaviour : Behaviour
{
    protected Coroutine StartCoroutine(IEnumerator routine)
    {
        return new();
    }

    protected void StopCoroutine(Coroutine routine)
    {
        
    }
}