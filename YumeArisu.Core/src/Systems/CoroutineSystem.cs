using YumeArisu.Core.Routines;
using YumeArisu.Core.Internal.YieldAbstraction;
using Silk.NET.Core;
using System.Diagnostics;

namespace YumeArisu.Core.Systems;

public class CoroutineSystem : SystemBase<CoroutineSystem, NoConfig>
{
    // 분류 파이프 라인
    private LinkedList<Coroutine> _unknownYields;
    private LinkedList<Coroutine> _coroutineYields;
    private LinkedList<Coroutine> _waitSecondsYields;
    private LinkedList<Coroutine> _waitFixedUpdateYields;
    private LinkedList<Coroutine> _waitUntilYields;

    private Dictionary<Coroutine, float> _waitingTimes;
    private HashSet<Coroutine> _rescheduleList; 

    // FixedUpdate
    // Update
    // AfterRender
    // WaitSeconds
    // WaitUntil

    internal override void StartUpInternal(NoConfig config)
    {
        _unknownYields = new();
        _coroutineYields = new();
        _waitSecondsYields = new();
        _waitFixedUpdateYields = new();
        _waitUntilYields = new();
        _waitingTimes = new();
        _rescheduleList = new();
    }

    internal override void ShutDownInternal()
    {
        _unknownYields.Clear();
        _unknownYields = null;
        _coroutineYields.Clear();
        _coroutineYields = null;
        _waitSecondsYields.Clear();
        _waitSecondsYields = null;
        _waitFixedUpdateYields.Clear();
        _waitFixedUpdateYields = null;
        _waitUntilYields.Clear();
        _waitUntilYields = null;
        _waitingTimes.Clear();
        _waitingTimes = null;
        _rescheduleList.Clear();
        _rescheduleList = null;
    }

    public void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine.Done)
            return;

        coroutine.Done = true;

        if (coroutine.SchedulerNode != null)
        {
            coroutine.SchedulerNode.List?.Remove(coroutine.SchedulerNode); // O(1)
            coroutine.SchedulerNode = null;
        }

        if (_waitingTimes.ContainsKey(coroutine))
            _waitingTimes.Remove(coroutine);
    }

    public void ProcessUnknownYields()
    {
        foreach (var coroutine in _unknownYields)
            ProcessCoroutine(coroutine);
    }

    public void ProcessWaitSecondsYields()
    {
        foreach (var coroutine in _waitSecondsYields)
        {
            if (_waitingTimes.TryGetValue(coroutine, out float waitTime) 
                && (double)waitTime < Time.HighResTotalTime)
                ProcessCoroutine(coroutine);
        }
    }

    public void ProcessWaitFixedUpdateYields()
    {
        foreach (var coroutine in _waitFixedUpdateYields)
            ProcessCoroutine(coroutine);
    }

    public void ProcessWaitUntilYields()
    {
        foreach (var coroutine in _waitSecondsYields)
        {
            if(((WaitUntil)coroutine.WaitOption)?.Condition() ?? true)
                ProcessCoroutine(coroutine);
        }
    }


    public void ImmediateStopAllCoroutines()
    {
        // TODO : 씬 전환을 대상으로 하기 때문에 전체 코루틴을 Stop 및 Discard해야 함.
    }

    public void UpdateSchedule()
    {
        foreach (var coroutine in _rescheduleList)
            Reschedule(coroutine);

        _rescheduleList.Clear();
    }

    internal void Reschedule(Coroutine coroutine)
    {
        coroutine.SchedulerNode.List?.Remove(coroutine.SchedulerNode);

        if (coroutine.Done)
            return;

        switch (coroutine.WaitOption)
        {
            case null:
                _unknownYields.AddLast(coroutine);
                break;
            case Coroutine other:
                other.SleepingWaifu(coroutine);
                _coroutineYields.AddLast(coroutine); 
                break;
            case WaitForSeconds sec:
                _waitingTimes.Add(coroutine, Time.TotalTime + sec.Seconds);
                _waitSecondsYields.AddLast(coroutine);
                break;
            case WaitForFixedUpdate:
                _waitFixedUpdateYields.AddLast(coroutine);
                break;
            case WaitUntil:
                _waitUntilYields.AddLast(coroutine);
                break;
        }
    }

    private void ProcessCoroutine(Coroutine coroutine)
    {
        if (coroutine.MoveNext())
        {
            _rescheduleList.Add(coroutine);
            return;
        }
        coroutine.SchedulerNode.List?.Remove(coroutine.SchedulerNode);
    }
}