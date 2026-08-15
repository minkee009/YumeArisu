namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct UniformMeta
{
    public UniformType Type { get; init; }
    public float[] Data { get; init; }
}