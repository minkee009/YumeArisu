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

    // 파이프라인 순회 스냅샷용 버퍼
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

    /// <summary>
    /// 코루틴을 시작시킵니다.
    /// </summary>
    internal Coroutine StartCoroutine(ScriptBehaviour owner, IEnumerator routine)
    {
        var coroutine = new Coroutine(owner, routine);
        _allActive.Add(coroutine);
        Proccess(coroutine);
        return coroutine;
    }

    /// <summary>
    /// 특정 코루틴을 정지시킵니다.
    /// </summary>
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

    /// <summary>
    /// 모든 파이프라인의 코루틴들을 즉시 정지시킵니다.
    /// </summary>
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

    /// <summary>
    /// wait fixed인 코루틴들을 전부 순회하며 진행시킵니다.
    /// </summary>
    public void YieldFixedUpdate()
    {
        var snapshot = SnapshotToBuffer(_fixedUpdateList, ref _fixedUpdateBuffer);
        foreach (var coroutine in snapshot)
            Proccess(coroutine);
    }
 
    /// <summary>
    /// wait update 혹은 wait unknown인 코루틴들을 전부 순회하며 진행시킵니다.
    /// </summary>
    public void YieldUpdate()
    {
        var snapshot = SnapshotToBuffer(_updateList, ref _updateBuffer);
        foreach (var coroutine in snapshot)
        {
            if (IsWaitEnd(coroutine))
                Proccess(coroutine);
        }
    }
 
    /// <summary>
    /// wait until인 코루틴들을 전부 순회하며 진행시킵니다.
    /// </summary>
    public void YieldUntil()
    {
        var snapshot = SnapshotToBuffer(_waitUntilList, ref _waitUntilBuffer);
        foreach (var coroutine in snapshot)
        {
            if (((WaitUntil)coroutine.WaitOption).Condition())
                Proccess(coroutine);
        }
    }

    /// <summary>
    /// 코루틴의 대기 상태가 끝났는지 확인합니다.
    /// </summary>
    /// <param name="coroutine"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 코루틴 파이프라인을 재분류하기 위해 코루틴이 현재 속한 리스트에서 제거합니다.
    /// </summary>
    /// <param name="coroutine">재분류 대상 코루틴</param>
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

    /// <summary>
    /// 코루틴에 담긴 루틴을 진행시킨 뒤 waitOption을 확인해 코루틴 파이프라인에 재분류시킵니다.
    /// </summary>
    /// <param name="coroutine"></param>
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
    /// 내부 순회용 코루틴 파이프라인 스냅샷을 제공합니다.
    /// </summary>
    /// <param name="list">복사할 파이프라인</param>
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