using System.Collections;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Routines;

public class ScriptBehaviour : Behaviour
{
    protected internal override void OnDetached()
    {
        base.OnDetached();
        StopAllCoroutine();
    }

    protected Coroutine StartCoroutine(IEnumerator routine) => CoroutineSystem.Instance.StartCoroutine(this, routine);
    protected void StopCoroutine(Coroutine routine) => CoroutineSystem.Instance.StopCoroutine(routine);
    internal protected void StopAllCoroutine() => CoroutineSystem.Instance.StopAllCoroutines(this);
}
