using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Internal.ResourceHandling;

public abstract class Resource
{
    public string Path { get; internal set; }
    
    protected bool IsLoaded { get; set; }

    internal IFileIO FileIO { get; set; }

    internal bool IsLoadedBySystem => FileIO != null && !string.IsNullOrEmpty(Path);

    internal bool Load(byte[] bytes)
    {
        if (IsLoaded)
            return false;

        IsLoaded = OnLoad(bytes);

        return IsLoaded;
    }

    internal void Unload()
    {
        if (!IsLoaded)
            return;
        
        OnUnload();

        IsLoaded = false;
    }

    protected abstract bool OnLoad(byte[] bytes);
    protected abstract void OnUnload();
}