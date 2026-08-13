using YumeArisu.Core.Abstractions;

namespace YumeArisu.Desktop.Implements;

public sealed class DebugFileIO : IFileIO
{
    public bool IsOpened => true;

    private string _rootFolder;

    public void SetRootFolder(string path) => _rootFolder = path;

    public bool Exists(string path)
    {
        if (string.IsNullOrEmpty(_rootFolder))
            throw new InvalidOperationException("루트 폴더가 설정되지 않았습니다. SetRootFolder를 먼저 호출하세요.");
    
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