using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Utility;

internal static class ResourceCheckExtensions
{
    internal static bool IsLoadedBySystem(this Resource resource) => resource.FileIO != null && !string.IsNullOrEmpty(resource.Path);
}