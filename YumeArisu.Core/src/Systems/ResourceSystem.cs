using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Systems;

public class ResourceSystem : SystemBase<ResourceSystem, IFileIO>
{
    private IFileIO _fileIO;
    private Dictionary<string, Resource> _resourceTable;

    internal override void OnStartUp(IFileIO fileIO)
    {
        _fileIO = fileIO;
        _resourceTable = new();
    }

    internal override void OnShutDown()
    {
        foreach(var res in _resourceTable.Values)
            res.Unload();

        _fileIO = null;
        _resourceTable = null;
    }

    public T GetResource<T>(string path) where T : Resource, new()
    {
        if (_resourceTable.TryGetValue(path, out var cached))
        {
            return (T)cached;
        }

        var resource = new T();
        if (resource.Load(_fileIO.ReadAllBytes(path)))
        {
            resource.Path = path;
            _resourceTable[path] = resource;
            return resource;
        }

        throw new Exception("리소스 로드에 실패했습니다.");
    }

    public void ReleaseResource<T>(T resource) where T : Resource
    {
        if (resource.Path == null || resource.Path == string.Empty)
            throw new Exception("경로를 알 수 없는 리소스를 해제하려 했습니다.");

        if (_resourceTable.TryGetValue(resource.Path, out var _))
        {
            _resourceTable.Remove(resource.Path);
            resource.Unload();
        }
    }
}

public static class Resources
{
    public static T Get<T>(string path) where T : Resource, new() => ResourceSystem.Instance.GetResource<T>(path);
    public static void Release<T>(T resource) where T : Resource => ResourceSystem.Instance.ReleaseResource(resource);
}