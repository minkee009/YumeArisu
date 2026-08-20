namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct ShaderMeta
{
    public VertexLayout VertexLayout { get; init; }
    public string VertBodyPath { get; init; }
    public string FragBodyPath { get; init; }
}