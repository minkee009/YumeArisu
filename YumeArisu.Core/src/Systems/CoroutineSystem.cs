using System.Collections;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Internal.YieldHandling;

namespace YumeArisu.Core.Systems;

public class CoroutineSystem : SystemBase<CoroutineSystem, NoConfig>
{
    private List<Coroutine> _allActive = new(); 
    private LinkedList<Coroutine> _updateList = new();
    private LinkedList<Coroutine> _fixedUpdateList = new();
    private LinkedList<Coroutine> _waitUntilList = new();
    private Dictionary<Coroutine, double> _waitUntilTime = new();

    private Coroutine[] _updateBuffer = Array.Empty<Coroutine>();
    private Coroutine[] _fixedUpdateBuffer = Array.Empty<Coroutine>();
    private Coroutine[] _waitUntilBuffer = Array.Empty<Coroutine>();

    internal override void OnStartUp(NoConfig config)
    {
        _allActive.Clear();
        _updateList.Clear();
        _fixedUpdateList.Clear();
        _waitUntilList.Clear();
        _waitUntilTime.Clear();
    }

    internal override void OnShutDown()
    {
        ImmediateStopAllCoroutines();

        _updateBuffer = Array.Empty<Coroutine>();
        _fixedUpdateBuffer = Array.Empty<Coroutine>();
        _waitUntilBuffer = Array.Empty<Coroutine>();
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
 
        RemoveFromCurrentList(coroutine);
        _allActive.Remove(coroutine);
    }

    /// <summary>
    /// 특정 owner가 시작시킨 코루틴들을 모두 정지시킵니다.
    /// </summary>
    internal void StopAllCoroutines(ScriptBehaviour owner)
    {
        for (int i = _allActive.Count - 1; i >= 0; i--)
        {
            var coroutine = _allActive[i];
            if (coroutine.Owner == owner)
                StopCoroutine(coroutine);
        }
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
        var snapshot = SnapshotToBuffer(_fixedUpdateList, ref _fixedUpdateBuffer);
        foreach (var coroutine in snapshot)
            Proccess(coroutine);
    }
 
    public void YieldUpdate()
    {
        var snapshot = SnapshotToBuffer(_updateList, ref _updateBuffer);
        foreach (var coroutine in snapshot)
        {
            if (IsWaitEnd(coroutine))
                Proccess(coroutine);
        }
    }
 
    public void YieldUntil()
    {
        var snapshot = SnapshotToBuffer(_waitUntilList, ref _waitUntilBuffer);
        foreach (var coroutine in snapshot)
        {
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

    private void RemoveFromCurrentList(Coroutine coroutine)
    {
        switch (coroutine.ListState)
        {
            case WaitListState.Update:
                _updateList.Remove(coroutine.Node); 
                break;
            case WaitListState.FixedUpdate:
                _fixedUpdateList.Remove(coroutine.Node);
                break;
            case WaitListState.WaitUntil:
                _waitUntilList.Remove(coroutine.Node); 
                break;
        }

        coroutine.ListState = WaitListState.None;
        coroutine.Node = null;
        _waitUntilTime.Remove(coroutine);
    }

    internal void Proccess(Coroutine coroutine)
    {
        RemoveFromCurrentList(coroutine);

        if (!coroutine.MoveNext())
        {
            _allActive.Remove(coroutine);
            return;
        }

        switch (coroutine.WaitOption)
        {
            case WaitForFixedUpdate:
                coroutine.Node = _fixedUpdateList.AddLast(coroutine);
                coroutine.ListState = WaitListState.FixedUpdate;
                break;
            case WaitUntil:
                coroutine.Node = _waitUntilList.AddLast(coroutine);
                coroutine.ListState = WaitListState.WaitUntil;
                break;
            case WaitForSeconds sec:
                _waitUntilTime[coroutine] = Time.HighResTotalTime + sec.Seconds; // MoveNext 이후, 새 값 기준
                coroutine.Node = _updateList.AddLast(coroutine);
                coroutine.ListState = WaitListState.Update;
                break;
            case Coroutine inner:
                if (inner.Done)
                    Proccess(coroutine); // 이미 끝나있었으면 즉시 재진입
                else
                    inner.AddWaiter(coroutine); // 리스트에 안 넣고 잠재움
                break;
            default:
                coroutine.Node = _updateList.AddLast(coroutine);
                coroutine.ListState = WaitListState.Update;
                break;
        }
    }

    /// <summary>
    /// 내부 순회용 코루틴 리스트 스냅샷을 제공합니다.
    /// </summary>
    /// <param name="list">복사할 리스트</param>
    /// <param name="buffer">복사된 스냅샷용 버퍼</param>
    /// <returns></returns>
    private static Span<Coroutine> SnapshotToBuffer(LinkedList<Coroutine> list, ref Coroutine[] buffer)
    {
        int count = list.Count;
        if (count == 0)
            return Span<Coroutine>.Empty;
 
        if (buffer.Length < count)
            buffer = new Coroutine[count]; 
 
        list.CopyTo(buffer, 0);
        return buffer.AsSpan(0, count);
    }
}