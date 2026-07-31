using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Systems;

public class ResourceSystem : SystemBase<ResourceSystem, IFileIO>
{
    private IFileIO _fileIO;
    private Dictionary<string, Dictionary<Type, CacheEntry>> _resourceTable;

    internal override void OnStartUp(IFileIO fileIO)
    {
        _fileIO = fileIO;
        _resourceTable = new();
    }

    internal override void OnShutDown()
    {
        foreach(var types in _resourceTable.Values)
            foreach(var cache in types.Values)
                cache.Resource.Unload();

        _fileIO = null;
        _resourceTable = null;
    }

    public T GetResource<T>(string path) where T : Resource, new()
    {
        if(typeof(T) == typeof(Resource))
            throw new Exception("추상 클래스로 리소스를 불러올 수 없습니다.");

        if (_resourceTable.TryGetValue(path, out var types)
            && types.TryGetValue(typeof(T), out var cached))
        {
            cached.RefCount++;
            return (T)cached.Resource;
        }

        var resource = new T();
        if (resource.Load(_fileIO.ReadAllBytes(path)))
        {
            resource.Path = path;

            if (!_resourceTable.TryGetValue(path, out types))
            {
                types = new();
                _resourceTable[path] = types;
            }
            types[typeof(T)] = new(resource, 1);

            return resource;
        }

        throw new Exception("리소스 로드에 실패했습니다.");
    }

    public void ReleaseResource(Resource resource)
    {
        if (resource == null)
            throw new Exception("null 리소스를 반납하려 했습니다.");

        if (resource.Path == null || resource.Path == string.Empty)
            throw new Exception("경로를 알 수 없는 리소스를 반납하려 했습니다.");

        var actualType = resource.GetType();

        if (_resourceTable.TryGetValue(resource.Path, out var types)
            && types.TryGetValue(actualType, out var cached))
        {
            if (!ReferenceEquals(cached.Resource, resource))
                throw new Exception($"캐시된 인스턴스와 다른 객체를 반납하려 했습니다: {resource.Path}");

            cached.RefCount--;
            if(cached.RefCount <= 0)
            {
                // 해당 타입 캐시 제거
                types.Remove(actualType);

                // 해당 경로로 타입이 더 이상 없으면 전체 리소스 테이블에서 제거 
                if(types.Count == 0)
                    _resourceTable.Remove(resource.Path);

                resource.Unload();
            }
        }
        else
        {
            throw new Exception($"등록되지 않은 리소스를 반납하려 했습니다: {resource.Path}");
        }
    }
}

// 문법 설탕용 클래스
public static class Resources
{
    public static T Get<T>(string path) where T : Resource, new() => ResourceSystem.Instance.GetResource<T>(path);
    public static void Release(Resource resource) => ResourceSystem.Instance.ReleaseResource(resource);
}