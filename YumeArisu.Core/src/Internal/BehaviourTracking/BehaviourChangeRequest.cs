using YumeArisu.Core.Routines;

namespace YumeArisu.Core.Internal.BehaviourTracking;

internal struct BehaviourChangeRequest
{
    public Behaviour Behaviour;
    public bool IsRegister;
}