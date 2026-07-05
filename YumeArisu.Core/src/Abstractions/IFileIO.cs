namespace YumeArisu.Core.Abstractions;

/// <summary>
/// OS, 플랫폼 별 파일 접근 및 관리를 위한 인터페이스 입니다.
/// </summary>
public interface IFileIO
{
    byte[] ReadAllBytes(string path);
}