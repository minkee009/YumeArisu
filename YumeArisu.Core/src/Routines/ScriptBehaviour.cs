using System.Collections;

namespace YumeArisu.Core.Routines;

public class ScriptBehaviour : Behaviour
{
    protected Coroutine StartCoroutine(IEnumerator routine) => new(this, routine);

    protected void StopCoroutine(Coroutine routine)
    {
        // TODO : 구현 해야함.
    }
}
