using System.Collections;
using YumeArisu.Core.Routines;

namespace YumeArisu.Core.Systems;

public class CoroutineSystem : SystemBase<CoroutineSystem, NoConfig>
{
    private List<Coroutine> _allActive = new(); 
    private List<Coroutine> _updateList = new();
    private List<Coroutine> _fixedUpdateList = new();
    private List<Coroutine> _waitUntilList = new();
    private Dictionary<Coroutine, double> _waitUntilTime = new();

    internal override void StartUpInternal(NoConfig config)
    {
        _allActive.Clear();
        _updateList.Clear();
        _fixedUpdateList.Clear();
        _waitUntilList.Clear();
        _waitUntilTime.Clear();
    }

    internal override void ShutDownInternal()
    {
        ImmediateStopAllCoroutines();
    }

    internal Coroutine StartCoroutine(ScriptBehaviour owner, IEnumerator routine)
    {
        var coroutine = new Coroutine(owner, routine);
        _allActive.Add(coroutine);
        Proccess(coroutine);
        return coroutine;
    }

    internal void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine.Done)
            return;

        if (coroutine.WaitOption is Coroutine inner)
            inner.RemoveWaiter(coroutine);

        coroutine.ForceStop(); 

        _updateList.Remove(coroutine);
        _fixedUpdateList.Remove(coroutine);
        _waitUntilList.Remove(coroutine);
        _waitUntilTime.Remove(coroutine);
    }

    public void ImmediateStopAllCoroutines()
    {
        foreach (var coroutine in _updateList) coroutine.ForceStop();
        foreach (var coroutine in _fixedUpdateList) coroutine.ForceStop();
        foreach (var coroutine in _waitUntilList) coroutine.ForceStop();

        _allActive.Clear();
        _updateList.Clear();
        _fixedUpdateList.Clear();
        _waitUntilList.Clear();
        _waitUntilTime.Clear();
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
            var coroutine = _waitUntilList[i];
            if (((WaitUntil)coroutine.WaitOption).Condition())
                Proccess(coroutine);
        }
    }

    private bool IsWaitEnd(Coroutine coroutine)
    {
        switch (coroutine.WaitOption)
        {
            case null:
                return true;
            case WaitForSeconds:
                return _waitUntilTime.TryGetValue(coroutine, out double t) && Time.HighResTotalTime >= t;
            default:
                return true;
        }
    }

    internal void Proccess(Coroutine coroutine)
    {
        _updateList.Remove(coroutine);
        _fixedUpdateList.Remove(coroutine);
        _waitUntilList.Remove(coroutine);
        _waitUntilTime.Remove(coroutine);

        if (!coroutine.MoveNext())
        {
            _allActive.Remove(coroutine);
            return;
        }

        switch (coroutine.WaitOption)
        {
            case WaitForFixedUpdate:
                _fixedUpdateList.Add(coroutine);
                break;
            case WaitUntil:
                _waitUntilList.Add(coroutine);
                break;        
            case WaitForSeconds sec:
                _waitUntilTime[coroutine] = Time.HighResTotalTime + sec.Seconds; // MoveNext 이후, 새 값 기준
                _updateList.Add(coroutine);
                break;
            case Coroutine inner:
                if (inner.Done)
                    Proccess(coroutine); // 이미 끝나있었으면 즉시 재진입
                else
                    inner.AddWaiter(coroutine); // 리스트에 안 넣고 잠재움
                break;
            default:
                _updateList.Add(coroutine);
                break;
        }
    }
}