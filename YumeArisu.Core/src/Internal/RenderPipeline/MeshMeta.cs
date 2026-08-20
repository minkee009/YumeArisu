namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct MeshMeta
{
    internal VertexLayout Layout { get; init; }
    internal float[] Vertices { get; init; }
    internal uint[] Indices { get; init; }
}