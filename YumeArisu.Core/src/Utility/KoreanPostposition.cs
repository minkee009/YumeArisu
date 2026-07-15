namespace YumeArisu.Core.Utility;

public static class KoreanJosa
{
    public static string GetEunNeun(string word)
    {
        return HasBatchim(word) ? "은" : "는";
    }

    public static string GetIGa(string word)
    {
        return HasBatchim(word) ? "이" : "가";
    }

    public static string GetEulReul(string word)
    {
        return HasBatchim(word) ? "을" : "를";
    }

    public static string GetEuroRo(string word)
    {
        return HasBatchim(word) ? "으로" : "로";
    }

    public static bool HasBatchim(string word)
    {
        if (string.IsNullOrEmpty(word))
            return false;

        char lastChar = word[^1];

        if (lastChar < '가' || lastChar > '힣')
            return false;

        return ((lastChar - '가') % 28) != 0;
    }
}