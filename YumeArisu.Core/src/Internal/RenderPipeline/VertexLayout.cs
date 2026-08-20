using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct VertexLayout
{
    public VertexElement[] Elements { get; init; }

    internal ulong GetID()
    {
        // 주의 : Elements 내부 원소가 정렬되어있다는 전제가 깔림!

        ulong hash = 14695981039346656037UL;

        foreach (var element in Elements)
        {
            hash ^= (uint)element.Location;
            hash *= 1099511628211UL;

            hash ^= (uint)element.Type;
            hash *= 1099511628211UL;
        }

        return hash;
    } 
}