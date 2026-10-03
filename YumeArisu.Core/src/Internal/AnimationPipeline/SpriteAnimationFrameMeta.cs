namespace YumeArisu.Core.Internal.AnimationPipeline;

internal readonly struct SpriteAnimationFrameMeta
{
    public string Sprite { get; init; }    // .sprite 경로
    public float? Duration { get; init; }  // 프레임 단위 유지 시간, 생략하면 1
    public string Event { get; init; }     // 이 프레임 진입 시 발생할 이벤트 이름 (선택)
}