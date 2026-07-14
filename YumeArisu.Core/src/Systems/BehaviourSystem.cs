using System.Runtime.CompilerServices;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Internal.BehaviourTracking;
using System.Reflection.Metadata;

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
    private HashSet<Behaviour> _activeBehaviours;
    private Queue<Behaviour> _enabledBehaviours;
    private Queue<Behaviour> _disabledBehaviours;
    private Queue<Action> _registerQueue;
    private bool _needSort = false;

    internal override void StartUpInternal(NoConfig control)
    {
        _behaviours = new();
        _activeBehaviours = new();
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
                _enabledBehaviours.Enqueue(bh);
                continue;
            }
            if(!bh.IsActiveAndEnabled && _activeBehaviours.Contains(bh))
            {
                _activeBehaviours.Remove(bh);
                _disabledBehaviours.Enqueue(bh);
            } 
        }
    }

    public void ExecuteAwake()
    {
        foreach(var bh in _behaviours)
        {
            if(bh.State == BehaviourState.Created 
                && bh.GameObject.ActiveInHierarchy)
            {
                bh.Awake();
                bh.State = BehaviourState.Awoken;
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
        foreach(var bh in _behaviours)
        {
            if(bh.State == BehaviourState.Awoken 
                && bh.IsActiveAndEnabled)
            {
                bh.Start();
                bh.State = BehaviourState.Started;
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
        for (int i = _behaviours.Count - 1; i >= 0; i--)
        {
            var bh = _behaviours[i];
            if (bh.IsPendingDestroy)
            {
                bh.OnDestroy();
                _behaviours.RemoveAt(i);
            }
        }
    }
}