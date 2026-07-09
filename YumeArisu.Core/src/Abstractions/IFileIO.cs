namespace YumeArisu.Core.Abstractions;

/// <summary>
/// 플랫폼 별 파일 접근 및 관리를 위한 인터페이스 입니다.
/// </summary>
public interface IFileIO
{
    FileIOFeatures Capabilities { get; }

    byte[] ReadAllBytes(string path);
    string ReadAllString(string path);
}



// 아래는 추후에 별도의 추상화 단계로 넘어가서 구현하기.
[Flags]
public enum FileIOFeatures
{
    None = 0,
    Read = 1 << 0,
    Write = 1 << 1,
    Streaming = 1 << 2,
    Async = 1 << 3
}