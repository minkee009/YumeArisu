namespace YumeArisu.Core.Internal.BehaviourTracking;

internal enum BehaviourState
{
    Created,        // AddComponent 직후, 아직 Awake 안 됨
    Awoken,         // Awake 완료, Start 대기 중
    Started,        // Start 완료, 정상 업데이트 중
    PendingDestroy, // Destory 예약됨
    Destroyed       // Destroy 됨
}