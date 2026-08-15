using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Internal.BehaviourTracking;

namespace YumeArisu.Core.Routines;

public abstract class Behaviour : Component
{
    public virtual int ExecutionOrder => 0;
    
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
                return;

            _enabled = value;
            MarkActiveChange();
        }
    }

    public bool IsActiveAndEnabled => Enabled && GameObject.ActiveInHierarchy;

    internal ExecutionPhase ExecutionPhase { get; set; } = ExecutionPhase.Created;
    internal bool IsPendingRemove { get; set; } = false;
    internal bool IsRegistered { get; set; } = false;
    internal bool IsScheduled { get; set; } = false;

    public virtual void OnAwake() { }
    public virtual void OnEnable() { }
    public virtual void OnStart() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnDisable() { }
    public virtual void OnRemove() { }

    private bool _enabled = true;

    protected internal override void OnAttach()
    {
        GameObject.ActiveInHierarchyChange += HandleActiveChange;
        BehaviourSystem.Instance.RegisterBehaviour(this);
    }

    protected internal override void OnDetach()
    {
        GameObject.ActiveInHierarchyChange -= HandleActiveChange;
        BehaviourSystem.Instance.UnregisterBehaviour(this);
    }

    internal void MarkActiveChange() => BehaviourSystem.Instance.MarkScheduleChange(this);

    private void HandleActiveChange(GameObject _) => MarkActiveChange();
}