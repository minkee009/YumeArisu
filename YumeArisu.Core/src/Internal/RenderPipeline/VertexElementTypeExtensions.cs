namespace YumeArisu.Core.Internal.RenderPipeline;

internal static class VertexElementTypeExtensions
{
    internal static int GetSize(this VertexElementType type) => type switch
    {
        VertexElementType.Float  => 4,
        VertexElementType.Float2 => 8,
        VertexElementType.Float3 => 12,
        VertexElementType.Float4 => 16,
        VertexElementType.Int => 4,
        VertexElementType.Int2 => 8,
        VertexElementType.Int3 => 12,
        VertexElementType.Int4 => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    internal static int GetComponentCount(this VertexElementType type) => type switch
    {
        VertexElementType.Float  => 1,
        VertexElementType.Float2 => 2,
        VertexElementType.Float3 => 3,
        VertexElementType.Float4 => 4,
        VertexElementType.Int => 1,
        VertexElementType.Int2 => 2,
        VertexElementType.Int3 => 3,
        VertexElementType.Int4 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}