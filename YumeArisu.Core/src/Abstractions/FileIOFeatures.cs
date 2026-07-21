namespace YumeArisu.Core.Abstractions;

// TODO : 아래는 추후에 별도의 추상화 단계로 넘어가서 구현하기.
[Flags]
public enum FileIOFeatures
{
    None = 0,
    Read = 1 << 0,
    Write = 1 << 1,
    Streaming = 1 << 2,
    Async = 1 << 3
}