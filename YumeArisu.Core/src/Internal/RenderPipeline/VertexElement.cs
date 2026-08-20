namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct VertexElement
{
    public int Location { get; init; }
    public string Name { get; init; }
    public VertexElementType Type { get; init; }
}