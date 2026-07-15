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

    private bool _enabled = true;

    protected internal override void OnAttached()
    {
        GameObject.OnActiveInHierarchyChange += HandleActiveChange;
        BehaviourSystem.Instance.RegisterBehaviour(this);
    }

    protected internal override void OnDetached()
    {
        GameObject.OnActiveInHierarchyChange -= HandleActiveChange;
        BehaviourSystem.Instance.UnregisterBehaviour(this);
    }

    internal void MarkActiveChange()
    {
        BehaviourSystem.Instance.MarkActiveStateChange(this);
    }

    private void HandleActiveChange(GameObject _) => MarkActiveChange();
}