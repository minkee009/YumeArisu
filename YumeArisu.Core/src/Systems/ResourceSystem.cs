using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class ResourceSystem : SystemBase<ResourceSystem, IFileIO>
{
    private IFileIO _fileIO;

    internal override void StartUpInternal(IFileIO fileIO)
    {
        _fileIO = fileIO;
    }

    internal override void ShutDownInternal()
    {
        _fileIO = null;
    }

}