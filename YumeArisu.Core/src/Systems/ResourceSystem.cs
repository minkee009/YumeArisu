using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Systems;

public class ResourceSystem : SystemBase<ResourceSystem, IFileIO>
{
    private IFileIO _fileIO;
    private Dictionary<string, CacheEntry> _resourceTable;

    internal override void OnStartUp(IFileIO fileIO)
    {
        _fileIO = fileIO;
        _resourceTable = new();
    }

    internal override void OnShutDown()
    {
        foreach (var cache in _resourceTable.Values)
                cache.Resource.Unload();

        _fileIO = null;
        _resourceTable = null;
    }

    /// <summary>
    /// 리소스를 가져옵니다. 처음으로 가져오는 리소스의 경우 리소스 테이블 캐시에 할당이 일어납니다.
    /// </summary>
    /// <typeparam name="T">반환받을 타입</typeparam>
    /// <param name="path">파일 경로</param>
    /// <returns>리소스</returns>
    /// <exception cref="Exception"></exception>
    public T GetResource<T>(string path) where T : Resource, new()
    {
        if (_resourceTable.TryGetValue(path, out var cached))
        {
            // 이미 있는 캐시가 T 타입이 아닌 경우 -> 에러, 단일 타입 리소스만 처리 가능
            if (typeof(T) != cached.Resource.GetType())
                throw new Exception($"리소스 타입 불일치: {path}는 이미 {cached.Resource.GetType().Name}로 로드됨");

            cached.RefCount++;
            return (T)cached.Resource;
        }

        var resource = new T { FileIO = _fileIO, Path = path };

        if (resource.Load(_fileIO.ReadAllBytes(path)))
        {
            _resourceTable[path] = new(resource, 1);
            return resource;
        }

        throw new Exception("리소스 로드에 실패했습니다.");
    }

    /// <summary>
    /// 리소스를 반환시킵니다. 더 이상 참조하는 객체가 없는 경우 리소스가 해제됩니다.
    /// </summary>
    /// <param name="resource">반환할 리소스</param>
    /// <exception cref="Exception"></exception>
    public void ReleaseResource(Resource resource)
    {
        if (resource is null)
            throw new Exception("null 리소스를 반납하려 했습니다.");

        if (string.IsNullOrEmpty(resource.Path))
            throw new Exception("경로를 알 수 없는 리소스를 반납하려 했습니다.");

        if (_resourceTable.TryGetValue(resource.Path, out var cached))
        {
            // 경로 주입 공격을 막는 방어
            if (!ReferenceEquals(cached.Resource, resource))
                throw new Exception($"캐시된 인스턴스와 다른 객체를 반납하려 했습니다: {resource.Path}");

            cached.RefCount--;
            if (cached.RefCount <= 0)
            {
                // 참조가 남아 있지 않는 경우 캐시에서 제거
                _resourceTable.Remove(resource.Path);
                resource.Unload();
            }
        }
        else
        {
            throw new Exception($"등록되지 않은 리소스를 반납하려 했습니다: {resource.Path}");
        }
    }

    /// <summary>
    /// 리소스가 리소스 테이블에 캐시되어 있는지 검사합니다.
    /// </summary>
    /// <param name="resource">검사할 리소스</param>
    /// <param name="refCount">리소스의 참조 카운트</param>
    /// <returns>리소스 테이블 내 캐시 여부</returns>
    public bool CacheCheck(Resource resource, out int refCount)
    {
        if (resource is null || string.IsNullOrEmpty(resource.Path))
        {
            refCount = -1;
            return false;
        }

        bool result = _resourceTable.TryGetValue(resource.Path, out var cached);
        refCount = result ? cached.RefCount : -1;

        return result;
    }
}

// 문법 설탕용 클래스
public static class Resources
{
    public static T Get<T>(string path) where T : Resource, new() => ResourceSystem.Instance.GetResource<T>(path);
    public static void Release(Resource resource) => ResourceSystem.Instance.ReleaseResource(resource);
    public static void CacheCheck(Resource resource, out int refCount) => ResourceSystem.Instance.CacheCheck(resource, out refCount);
}