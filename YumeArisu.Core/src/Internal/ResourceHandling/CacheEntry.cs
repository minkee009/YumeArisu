namespace YumeArisu.Core.Internal.ResourceHandling;

internal class CacheEntry
{
    public Resource Resource { get; private set; }
    public int RefCount { get; internal set; }

    internal CacheEntry(Resource resource, int refCount)
    {
        Resource = resource;
        RefCount = refCount;
    }
}