namespace YumeArisu.Core.Routines;

/// <summary>
/// 엔진이 예약한 Behaviour 실행 순서입니다. 값이 작을수록 먼저 실행됩니다.
/// 예약 값에 오프셋을 더해 사이에 끼울 수 있습니다. (예: Animation + 1)
/// 구간 간격은 1000이므로 오프셋은 ±999 이내로 쓰세요.
/// </summary>
public static class ReservedExecutionOrder
{
    /// <summary>일반 스크립트 (Behaviour 기본값)</summary>
    public const int Default = 0;

    /// <summary>일반 스크립트가 끝난 뒤 결과를 덮어쓰는 컴포넌트 (SpriteAnimator 등)</summary>
    public const int Animation = 1000;
}