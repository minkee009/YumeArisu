using System.Collections;
using YumeArisu.Core.Routines;

namespace YumeArisu.Core.Systems;

public class CoroutineSystem : SystemBase<CoroutineSystem, NoConfig>
{
    private List<Coroutine> _updateList = new();
    private List<Coroutine> _fixedUpdateList = new();
    private List<Coroutine> _waitUntilList = new();
    private Dictionary<Coroutine, float> _waitUntilTime = new();

    internal override void StartUpInternal(NoConfig config)
    {
        _updateList.Clear();
        _fixedUpdateList.Clear();
        _waitUntilList.Clear();
        _waitUntilTime.Clear();
    }

    internal override void ShutDownInternal()
    {
        _updateList.Clear();
        _fixedUpdateList.Clear();
        _waitUntilList.Clear();
        _waitUntilTime.Clear();
    }

    internal Coroutine StartCoroutine(ScriptBehaviour owner, IEnumerator routine)
    {
        var coroutine = new Coroutine(owner, routine);
        Proccess(coroutine);
        return coroutine;
    }

    internal void StopCoroutine(Coroutine coroutine)
    {
        _updateList.Remove(coroutine);
        _fixedUpdateList.Remove(coroutine);
        _waitUntilList.Remove(coroutine);
        _waitUntilTime.Remove(coroutine);
    }

    public void YieldFixedUpdate()
    {
        for (int i = _fixedUpdateList.Count - 1; i >= 0; i--)
            Proccess(_fixedUpdateList[i]);
    }

    public void YieldUpdate()
    {
        for (int i = _updateList.Count - 1; i >= 0; i--)
        {
            var coroutine = _updateList[i];
            if (IsWaitEnd(coroutine))
                Proccess(coroutine);
        }
    }

    public void YieldUntil()
    {
        for (int i = _waitUntilList.Count - 1; i >= 0; i--)
        {
            var c = _waitUntilList[i];
            if (((WaitUntil)c.WaitOption).Condition())
                Proccess(c);
        }
    }

    private bool IsWaitEnd(Coroutine coroutine) => coroutine.WaitOption switch
    {
        null => true,
        WaitForSeconds => _waitUntilTime.TryGetValue(coroutine, out float t) && Time.TotalTime >= t,
        Coroutine inner => inner.Done,
        _ => true
    };

    private void Proccess(Coroutine coroutine)
    {
        _updateList.Remove(coroutine);
        _fixedUpdateList.Remove(coroutine);
        _waitUntilList.Remove(coroutine);
        _waitUntilTime.Remove(coroutine);

        if (coroutine.WaitOption is WaitForSeconds sec)
            _waitUntilTime[coroutine] = Time.TotalTime + sec.Seconds;

        if (!coroutine.MoveNext())
            return;

        switch (coroutine.WaitOption)
        {
            case WaitForFixedUpdate:
                _fixedUpdateList.Add(coroutine);
                break;
            case WaitUntil:
                _waitUntilList.Add(coroutine);
                break;
            default:
                _updateList.Add(coroutine);
                break;
        }
    }
}