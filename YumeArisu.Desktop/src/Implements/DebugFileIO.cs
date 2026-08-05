using System.ComponentModel.Design.Serialization;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Desktop.Implements;

public sealed class DebugFileIO : IFileIO
{
    private string _rootFolder;

    public void SetRootFolder(string path) => _rootFolder = path;

    public bool Exists(string path)
    {
        return File.Exists(Path.Combine(_rootFolder, path));
    }

    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(Path.Combine(_rootFolder, path));
    }

    public string ReadAllString(string path)
    {
        return File.ReadAllText(Path.Combine(_rootFolder, path));
    }

    public void Dispose() { }
}