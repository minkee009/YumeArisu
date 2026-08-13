using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Internal.ResourceHandling;

public abstract class Resource
{
    public string Path { get; internal set; }

    internal IFileIO FileIO { get; set; }

    private bool _isLoaded = false;

    internal bool Load(byte[] bytes)
    {
        if (_isLoaded)
            return false;

        _isLoaded = OnLoad(bytes);

        return _isLoaded;
    }

    internal void Unload()
    {
        if (!_isLoaded)
            return;
        
        OnUnload();

        _isLoaded = false;
    }

    protected abstract bool OnLoad(byte[] bytes);
    protected abstract void OnUnload();
}