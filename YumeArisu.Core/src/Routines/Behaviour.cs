using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Internal.BehaviourTracking;

namespace YumeArisu.Core.Routines;

public abstract class Behaviour : Component
{
    public virtual int ExecutionOrder => 0;
    public bool Enabled { get; set; } = true;
    public bool IsActiveAndEnabled => Enabled && GameObject.ActiveInHierarchy;

    internal BehaviourState State { get; set; } = BehaviourState.Created;
    internal bool IsPendingDestroy { get; set; } = false;

    public virtual void Awake() { }
    public virtual void OnEnable() { }
    public virtual void Start() { }
    public virtual void FixedUpdate() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
    public virtual void OnDisable() { }
    public virtual void OnDestroy() { }

    protected internal override void OnAttached()
    {
        GameObject.OnActiveInHierarchyChange += HandleHierarchyStateChange;
        BehaviourSystem.Instance.RegisterBehaviour(this);
    }

    protected internal override void OnDetached()
    {
        GameObject.OnActiveInHierarchyChange -= HandleHierarchyStateChange;
        BehaviourSystem.Instance.UnregisterBehaviour(this);
    }

    internal void MarkHierarchyStateChange()
    {
        BehaviourSystem.Instance.MarkHierarchyStateChange(this);
    }

    private void HandleHierarchyStateChange(GameObject _) => MarkHierarchyStateChange();
}