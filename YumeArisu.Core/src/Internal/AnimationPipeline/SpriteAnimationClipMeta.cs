using System.Text.Json.Serialization;

namespace YumeArisu.Core.Internal.AnimationPipeline;

internal readonly struct SpriteAnimationClipMeta
{
    public float FrameRate { get; init; }
    public string Mode { get; init; }      // "Once" | "Loop" | "PingPong", 생략하면 Loop
    public SpriteAnimationFrameMeta[] Frames { get; init; }
}