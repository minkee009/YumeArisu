using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Routines;

public abstract class Behaviour : Component
{
    public bool Enabled { get; set; } //set은 Behaviour 매니저 생성 이후
    public bool IsActiveAndEnabled => Enabled && GameObject.ActiveInHierarchy;

    public virtual void Awake() { }
    public virtual void OnEnable() { }
    public virtual void Start() { }
    public virtual void FixedUpdate() { }
    public virtual void Update() { }
    public virtual void OnDisable() { }
    public virtual void OnDestroy() { }

    protected internal override void OnAttached()
    {
        BehaviourSystem.Instance.RegisterBehaviour(this);
    }

    protected internal override void OnDetached()
    {
        BehaviourSystem.Instance.UnregisterBehaviour(this);
    }
}