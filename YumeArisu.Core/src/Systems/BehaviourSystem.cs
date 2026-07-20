using YumeArisu.Core.Routines;
using YumeArisu.Core.Internal.BehaviourTracking;

namespace YumeArisu.Core.Systems;

/*
    -------------------------------------------------
    Behaviour System - 주의사항 
    -------------------------------------------------
    
    Awake <- GameObject의 ActiveInHierarchy에만 의존함
    Start <- ActiveInHierarchy + Behaviour의 Enabled 모두 의존함

    active 상태 -> ActiveInHierarchy + Enabled가 모두 활성상태임.
    inactive 상태 -> 둘 중 하나라도 거짓임.

    [ ! ] - Application을 짤 때 아래 순서가 보장되어야 함.

    1. Awake
    2. OnEnable
    3. Start
    4. FixedUpdate
    5. Update
    6. LateUpdate
    7. OnDisable
    8. OnDestroy

    -------------------------------------------------
*/

public class BehaviourSystem : SystemBase<BehaviourSystem, NoConfig>
{   
    private List<Behaviour> _behaviours = new();
    private List<Behaviour> _scheduledBehaviours = new();
    private Queue<Behaviour> _pendingAwake = new();
    private Queue<Behaviour> _pendingStart = new();
    private Queue<Behaviour> _pendingDestroy = new();
    private Queue<Behaviour> _pendingOnEnable = new();
    private Queue<Behaviour> _pendingOnDisable = new();
    private HashSet<Behaviour> _markedForScheduleCheck = new();  
    private Queue<Behaviour> _registrationQueue = new();
    private Queue<Behaviour> _unregistrationQueue = new();
    private bool _needSort = false;
    private bool _needScheduleRebuild = false;

    internal override void StartUpInternal(NoConfig control)
    {
        _behaviours.Clear();
        _scheduledBehaviours.Clear();
        _pendingAwake.Clear();
        _pendingStart.Clear();
        _pendingDestroy.Clear();
        _pendingOnEnable.Clear();
        _pendingOnDisable.Clear();        
        _markedForScheduleCheck.Clear();
        _registrationQueue.Clear();
        _unregistrationQueue.Clear();
    }

    internal override void ShutDownInternal()
    {
        _behaviours.Clear();
        _scheduledBehaviours.Clear();
        _pendingAwake.Clear();
        _pendingStart.Clear();
        _pendingDestroy.Clear();
        _pendingOnEnable.Clear();
        _pendingOnDisable.Clear();        
        _markedForScheduleCheck.Clear();
        _registrationQueue.Clear();
        _unregistrationQueue.Clear();
    }

    internal void RegisterBehaviour(Behaviour bh)
    {
        _registrationQueue.Enqueue(bh);
    }

    internal void UnregisterBehaviour(Behaviour bh)
    {
        _unregistrationQueue.Enqueue(bh);
    }

    public void BeginFrame()
    {
        while (_registrationQueue.Count > 0)
        {
            var bh = _registrationQueue.Dequeue();
            _behaviours.Add(bh);
            _pendingAwake.Enqueue(bh);
            _pendingStart.Enqueue(bh);
            _markedForScheduleCheck.Add(bh);
            bh.IsRegistered = true;
            _needSort = true;
        }

        while (_unregistrationQueue.Count > 0)
        {
            var bh = _unregistrationQueue.Dequeue();
            bh.Enabled = false;
            bh.IsPendingDestroy = true;
            _pendingDestroy.Enqueue(bh);
            _markedForScheduleCheck.Add(bh);
        }

        if (_needSort)
        {
            SortBehaviours(_behaviours);
            _needSort = false;
            _needScheduleRebuild = true;
        }

        CheckScheduleChange();

        if (_needScheduleRebuild)
        {
            RebuildSchedule();
            _needScheduleRebuild = false;
        }
    }

    internal void MarkScheduleChange(Behaviour bh)
    {
        _markedForScheduleCheck.Add(bh);
    }

    private void CheckScheduleChange()
    {
        if (_markedForScheduleCheck.Count <= 0)
            return;

        foreach (var bh in _markedForScheduleCheck)
        {
            if (!bh.IsRegistered)
            {
                _scheduledBehaviours.Remove(bh);
                continue;
            }
            if (bh.IsActiveAndEnabled && !bh.IsScheduled)
            {
                bh.IsScheduled = true;
                _scheduledBehaviours.Add(bh);
                _pendingOnEnable.Enqueue(bh);
                continue;
            }
            if (!bh.IsActiveAndEnabled && bh.IsScheduled)
            {
                bh.IsScheduled = false;
                _scheduledBehaviours.Remove(bh);
                _pendingOnDisable.Enqueue(bh);
            }
        }

        _markedForScheduleCheck.Clear();
    }

    private void RebuildSchedule()
    {
        _scheduledBehaviours.Clear();
        foreach (var bh in _behaviours)          // 이미 ExecutionOrder로 정렬된 소스
        {
            if (bh.IsScheduled)
                _scheduledBehaviours.Add(bh);
        }
    }

    public void ExecuteAwake()
    {
        var flushCount = _pendingAwake.Count;
        for (int i = 0; i < flushCount; i++)
        {
            var bh = _pendingAwake.Dequeue();
            if (bh.GameObject.ActiveInHierarchy)
            {
                bh.Awake();
                bh.ExecutionPhase = ExecutionPhase.Awoken;
            }
            else if (bh.ExecutionPhase != ExecutionPhase.Awoken && !bh.IsPendingDestroy)
            {
                _pendingAwake.Enqueue(bh);
            }
        }
    }

    public void ExecuteOnEnable()
    {
        while (_pendingOnEnable.Count > 0)
            _pendingOnEnable.Dequeue().OnEnable();
    }

    public void ExecuteStart()
    {
        var flushCount = _pendingStart.Count;
        for (int i = 0; i < flushCount; i++)
        {
            var bh = _pendingStart.Dequeue();
            if (bh.IsActiveAndEnabled)
            {
                bh.Start();
                bh.ExecutionPhase = ExecutionPhase.Started;
            }
            else if (bh.ExecutionPhase != ExecutionPhase.Started && !bh.IsPendingDestroy)
            {
                _pendingStart.Enqueue(bh);
            }
        }
    }

    public void ExecuteFixedUpdate()
    {
        foreach (var bh in _scheduledBehaviours)
            bh.FixedUpdate();
    }

    public void ExecuteUpdate()
    {
        foreach (var bh in _scheduledBehaviours)
            bh.Update();
    }

    public void ExecuteLateUpdate()
    {
        foreach (var bh in _scheduledBehaviours)
            bh.LateUpdate();
    }

    public void ExecuteOnDisable()
    {
        while (_pendingOnDisable.Count > 0)
            _pendingOnDisable.Dequeue().OnDisable();
    }

    public void ExecuteOnDestroy()
    {
        while (_pendingDestroy.Count > 0)
        {
            var bh = _pendingDestroy.Dequeue();
            if (bh.IsPendingDestroy)
            {
                // Awake가 한 번이라도 실행 된 Behaviour들만 허용
                if (bh.ExecutionPhase != ExecutionPhase.Created)
                    bh.OnDestroy();
                bh.IsRegistered = false;
                _behaviours.Remove(bh);
            }
        }
    }

    private static void SortBehaviours(List<Behaviour> behaviours)
    {
        if (behaviours.Count > 1)
            behaviours.Sort((a,b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
    }
}