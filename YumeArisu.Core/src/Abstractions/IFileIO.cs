namespace YumeArisu.Core.Abstractions;

/// <summary>
/// 플랫폼 별 파일 접근 및 관리를 위한 인터페이스 입니다.
/// </summary>
public interface IFileIO : IDisposable
{
    public bool IsOpened { get; }
    public bool Exists(string path);
    byte[] ReadAllBytes(string path);
    string ReadAllString(string path);
}