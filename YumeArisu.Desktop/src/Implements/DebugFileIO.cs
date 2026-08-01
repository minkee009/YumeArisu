using YumeArisu.Core.Abstractions;

namespace YumeArisu.Desktop.Implements;

public sealed class DebugFileIO : IFileIO
{
    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    public string ReadAllString(string path)
    {
        return File.ReadAllText(path);
    }

    public void Dispose() { }
}