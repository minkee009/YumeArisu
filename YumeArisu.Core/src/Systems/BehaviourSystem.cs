using YumeArisu.Core.Routines;
using YumeArisu.Core.Internal.BehaviourTracking;

namespace YumeArisu.Core.Systems;

/*
    -------------------------------------------------
    Behaviour System - 주의사항 
    -------------------------------------------------
    
    Awake <- GameObject의 ActiveInHierarchy에만 의존함
    Start <- ActiveInHierarchy + Behaviour의 Enabled 모두 의존함

    active 상태 -> ActiveInHierarchy + Behaviour의 Enabled가 모두 활성상태임.
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
    private struct BehaviourChangeRequest
    {
        public Behaviour Behaviour;
        public bool IsRegister;
    } 
    private List<Behaviour> _behaviours;
    private HashSet<Behaviour> _behaviourSet;
    private HashSet<Behaviour> _activeBehaviourSet;
    private List<Behaviour> _activeBehaviours;
    private HashSet<Behaviour> _pendingActiveChangeBehaviours;
    private Queue<Behaviour> _pendingAwakeBehaviours;
    private Queue<Behaviour> _pendingStartBehaviours;
    private Queue<Behaviour> _pendingDestroyBehaviours;
    private Queue<Behaviour> _enabledBehaviours;
    private Queue<Behaviour> _disabledBehaviours;
    private Queue<BehaviourChangeRequest> _registerQueue;
    private bool _needSort = false;

    internal override void StartUpInternal(NoConfig control)
    {
        _behaviourSet = new();
        _behaviours = new();
        _activeBehaviourSet = new();
        _activeBehaviours = new();
        _pendingActiveChangeBehaviours = new();
        _pendingAwakeBehaviours = new();
        _pendingStartBehaviours = new();
        _pendingDestroyBehaviours = new();
        _enabledBehaviours = new();
        _disabledBehaviours = new();
        _registerQueue = new();
    }

    internal override void ShutDownInternal()
    {
        _registerQueue.Clear();
        _behaviourSet.Clear();
        _behaviours.Clear();
        _activeBehaviourSet.Clear();
        _activeBehaviours.Clear();
        _pendingActiveChangeBehaviours.Clear();
        _pendingAwakeBehaviours.Clear();
        _pendingStartBehaviours.Clear();
        _pendingDestroyBehaviours.Clear();
        _enabledBehaviours.Clear();
        _disabledBehaviours.Clear();
        _behaviourSet = null;
        _behaviours = null;
        _activeBehaviourSet = null;
        _activeBehaviours = null;
        _pendingActiveChangeBehaviours = null;
        _pendingAwakeBehaviours = null;
        _pendingStartBehaviours = null;
        _pendingDestroyBehaviours = null;
        _enabledBehaviours = null;
        _disabledBehaviours = null;
        _registerQueue = null;
    }

    internal void RegisterBehaviour(Behaviour bh)
    {
        _registerQueue.Enqueue(new BehaviourChangeRequest
        {
            Behaviour = bh,
            IsRegister = true,
        });
    }

    internal void UnregisterBehaviour(Behaviour bh)
    {
        _registerQueue.Enqueue(new BehaviourChangeRequest
        {
            Behaviour = bh,
            IsRegister = false,
        });
    }

    internal void MarkActiveStateChange(Behaviour bh)
    {
        _pendingActiveChangeBehaviours.Add(bh);
    }

    public void BeginFrame()
    {
        while (_registerQueue.Count > 0)
        {
            var request = _registerQueue.Dequeue();
            if (request.IsRegister)
            {
                _behaviourSet.Add(request.Behaviour);
                _behaviours.Add(request.Behaviour);
                _pendingAwakeBehaviours.Enqueue(request.Behaviour);
                _pendingStartBehaviours.Enqueue(request.Behaviour);
                _pendingActiveChangeBehaviours.Add(request.Behaviour);
                _needSort = true;
            }
            else
            {
                request.Behaviour.Enabled = false;
                request.Behaviour.IsPendingDestroy = true;
                _pendingDestroyBehaviours.Enqueue(request.Behaviour);
                _pendingActiveChangeBehaviours.Add(request.Behaviour);
            }
        }

        if (_needSort)
        {
            SortBehaviours(_behaviours);
            _needSort = false;
        }

        CheckChangeState();
    }

    private void CheckChangeState()
    {
        if (_pendingActiveChangeBehaviours.Count <= 0)
            return;

        foreach (var bh in _pendingActiveChangeBehaviours)
        {
            if (!_behaviourSet.Contains(bh))
            {
                _activeBehaviourSet.Remove(bh);
                _activeBehaviours.Remove(bh);
                continue;
            }

            if (bh.IsActiveAndEnabled && !_activeBehaviourSet.Contains(bh))
            {
                _activeBehaviourSet.Add(bh);
                _activeBehaviours.Add(bh);
                _enabledBehaviours.Enqueue(bh);
                continue;
            }
            if (!bh.IsActiveAndEnabled && _activeBehaviourSet.Contains(bh))
            {
                _activeBehaviourSet.Remove(bh);
                _activeBehaviours.Remove(bh);
                _disabledBehaviours.Enqueue(bh);
            }
        }

        _pendingActiveChangeBehaviours.Clear();
    }

    public void ExecuteAwake()
    {
        var flushCount = _pendingAwakeBehaviours.Count;
        for (int i = 0; i < flushCount; i++)
        {
            var bh = _pendingAwakeBehaviours.Dequeue();
            if (bh.GameObject.ActiveInHierarchy)
            {
                bh.Awake();
                bh.State = BehaviourState.Awoken;
            }
            else if (bh.State != BehaviourState.Awoken && !bh.IsPendingDestroy)
            {
                _pendingAwakeBehaviours.Enqueue(bh);
            }
        }
    }

    public void ExecuteOnEnable()
    {
        while (_enabledBehaviours.Count > 0)
            _enabledBehaviours.Dequeue().OnEnable();
    }

    public void ExecuteStart()
    {
        var flushCount = _pendingStartBehaviours.Count;
        for (int i = 0; i < flushCount; i++)
        {
            var bh = _pendingStartBehaviours.Dequeue();
            if (bh.IsActiveAndEnabled)
            {
                bh.Start();
                bh.State = BehaviourState.Started;
            }
            else if (bh.State != BehaviourState.Started && !bh.IsPendingDestroy)
            {
                _pendingStartBehaviours.Enqueue(bh);
            }
        }
    }

    public void ExecuteFixedUpdate()
    {
        foreach (var bh in _activeBehaviours)
            bh.FixedUpdate();
    }

    public void ExecuteUpdate()
    {
        foreach (var bh in _activeBehaviours)
            bh.Update();
    }

    public void ExecuteLateUpdate()
    {
        foreach (var bh in _activeBehaviours)
            bh.LateUpdate();
    }

    public void ExecuteOnDisable()
    {
        while (_disabledBehaviours.Count > 0)
            _disabledBehaviours.Dequeue().OnDisable();
    }

    public void ExecuteOnDestroy()
    {
        while (_pendingDestroyBehaviours.Count > 0)
        {
            var bh = _pendingDestroyBehaviours.Dequeue();
            if (bh.IsPendingDestroy)
            {
                // Awake가 한 번이라도 실행 된 Behaviour들만 허용
                if (bh.State != BehaviourState.Created)
                    bh.OnDestroy();
                _behaviourSet.Remove(bh);
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