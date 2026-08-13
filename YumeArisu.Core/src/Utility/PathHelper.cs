namespace YumeArisu.Core.Utility;

public static class PathHelper
{
    // 대소문자 구분 없이 확장자 확인 (.NET Core 2.1+)
    public static bool HasExtension(string filePath, string extensionWithDot)
    {
        if (string.IsNullOrEmpty(filePath)) return false;

        // 1. Path.GetExtension에 ReadOnlySpan<char> 전달 (힙 할당 발생 없음)
        ReadOnlySpan<char> ext = Path.GetExtension(filePath.AsSpan());

        // 2. MemoryExtensions.Equals를 활용한 zero-allocation 문자열 비교
        return ext.Equals(extensionWithDot.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }
}