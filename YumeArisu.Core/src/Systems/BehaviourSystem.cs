using System.Runtime.CompilerServices;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Internal.BehaviourTracking;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

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
    private List<Behaviour> _behaviours;
    private HashSet<Behaviour> _activeBehaviours;  // TODO : 1. 해쉬 키로만 사용하고 Iterator로 쓰지 않기
    private PriorityQueue<Behaviour, int> _pendingAwakeBehaviours;      // TODO : ▼ Priority Queue를 가진 하위 코드들 전부 힙할당이 자주 일어나는 지 파악하기, 일어난다면 Queue가 아닌 자료구조로 쉽게 해결할 수 있는지 파악
    private PriorityQueue<Behaviour, int> _pendingStartBehaviours;      //  *
    private PriorityQueue<Behaviour, int> _pendingDestroyBehaviours;    //  *
    private PriorityQueue<Behaviour, int> _enabledBehaviours;           //  *
    private PriorityQueue<Behaviour, int> _disabledBehaviours;          //  *
    private Queue<Action> _registerQueue;   // TODO : 2. Action + 람다로 불필요한 힙 쓰지 않게 Action이 아니거나 람다가 아닌 구조를 생각해보기
    private bool _needSort = false;

    internal override void StartUpInternal(NoConfig control)
    {
        _behaviours = new();
        _activeBehaviours = new();
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
        _behaviours.Clear();
        _activeBehaviours.Clear();
        _enabledBehaviours.Clear();
        _disabledBehaviours.Clear();
        _behaviours = null;
        _activeBehaviours = null;
        _enabledBehaviours = null;
        _disabledBehaviours = null;
        _registerQueue = null;
    }

    internal void RegisterBehaviour(Behaviour bh)
    {
        _registerQueue.Enqueue(
            () => 
            {
                _behaviours.Add(bh);
                _pendingAwakeBehaviours.Enqueue(bh, bh.ExecutionOrder);
                _pendingStartBehaviours.Enqueue(bh, bh.ExecutionOrder);
                _needSort = true;
            }
        );
    }

    internal void UnregisterBehaviour(Behaviour bh)
    {
        _registerQueue.Enqueue(
            () => 
            {
                bh.Enabled = false;
                bh.IsPendingDestroy = true;
                _pendingDestroyBehaviours.Enqueue(bh, bh.ExecutionOrder);
            }
        );
    }

    public void BeginFrame()
    {
        // 등록/해지 큐 비우기
        while(_registerQueue.Count > 0)
            _registerQueue.Dequeue()();

        // 정렬 시작
        if(_needSort)
        {
            // execution order대로 behaviours를 조정
            _behaviours.Sort((a,b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
            _needSort = false;
        }

        CheckChangeState();
    }

    private void CheckChangeState()
    {
        foreach(var bh in _behaviours)
        {
            if(bh.IsActiveAndEnabled && !_activeBehaviours.Contains(bh))
            {
                _activeBehaviours.Add(bh);
                _enabledBehaviours.Enqueue(bh, bh.ExecutionOrder);
                continue;
            }
            if(!bh.IsActiveAndEnabled && _activeBehaviours.Contains(bh))
            {
                _activeBehaviours.Remove(bh);
                _disabledBehaviours.Enqueue(bh, bh.ExecutionOrder);
            } 
        }
    }

    public void ExecuteAwake()
    {
        var flushCount = _pendingAwakeBehaviours.Count;
        for(int i = 0; i < flushCount; i++)
        {
            var bh = _pendingAwakeBehaviours.Dequeue();
            if(bh.GameObject.ActiveInHierarchy)
            {
                bh.Awake();
                bh.State = BehaviourState.Awoken;
            }
            else if(bh.State == BehaviourState.Created && !bh.IsPendingDestroy)
            {
                _pendingAwakeBehaviours.Enqueue(bh, bh.ExecutionOrder);
            }
        }
    }

    public void ExecuteOnEnable()
    {
        while(_enabledBehaviours.Count > 0)
        {
            _enabledBehaviours.Dequeue().OnEnable();
        }
    }

    public void ExecuteStart()
    {
        var flushCount = _pendingStartBehaviours.Count;
        for(int i = 0; i < flushCount; i++)
        {
            var bh = _pendingStartBehaviours.Dequeue();
            if(bh.IsActiveAndEnabled)
            {
                bh.Start();
                bh.State = BehaviourState.Started;
            }
            else if(bh.State == BehaviourState.Awoken && !bh.IsPendingDestroy)
            {
                _pendingStartBehaviours.Enqueue(bh, bh.ExecutionOrder);
            }
        }
    }

    public void ExecuteFixedUpdate()
    {
        foreach(var bh in _activeBehaviours)
        {
            bh.FixedUpdate();
        }
    }

    public void ExecuteUpdate()
    {
        foreach(var bh in _activeBehaviours)
        {
            bh.Update();
        }
    }

    public void ExecuteLateUpdate()
    {
        foreach(var bh in _activeBehaviours)
        {
            bh.LateUpdate();
        }
    }

    public void ExecuteOnDisable()
    {
        while(_disabledBehaviours.Count > 0)
        {
            _disabledBehaviours.Dequeue().OnDisable();
        }
    }

    public void ExecuteOnDestroy()
    {
        while(_pendingDestroyBehaviours.Count > 0)
        {
            var bh = _pendingDestroyBehaviours.Dequeue();
            if(bh.IsPendingDestroy)
            {
                bh.OnDestroy();
                _behaviours.Remove(bh);
            }
        }
    }
}