namespace YumeArisu.Core.Utility;

public class ConsoleMessages
{
    public static void PrintProgress(int current, int total, string msg)
    {
        const int barWidth = 30;
        double ratio = (double)current / total;
        int filled = (int)(barWidth * ratio);

        string bar = new string('#', filled) + new string('-', barWidth - filled);
        string percent = (ratio * 100).ToString("F1");
        string prefix = $"[{bar}] {percent}% ({current}/{total}) ";

        // 콘솔 너비 측정 (오버플로우 방지를 위해 2~3칸 여유 확보)
        int consoleWidth = Console.IsOutputRedirected ? 120 : Console.WindowWidth;
        int targetWidth = Math.Max(consoleWidth - 2, 20);

        int prefixWidth = GetDisplayWidth(prefix);
        int availableWidth = targetWidth - prefixWidth;

        // 파일 경로를 화면 표시 너비에 맞춰 자르기
        string fileDisplay = TruncateToDisplayWidth(msg, availableWidth);

        string line = prefix + fileDisplay;
        int currentLineWidth = GetDisplayWidth(line);

        // 잔여 잔상 지우기용 공백 패딩
        int paddingCount = Math.Max(0, targetWidth - currentLineWidth);
        string paddedLine = line + new string(' ', paddingCount);

        Console.Write("\r" + paddedLine);
    }

    /// <summary>
    /// 한글/전각 문자를 고려한 실제 콘솔 출력 너비를 계산합니다.
    /// </summary>
    private static int GetDisplayWidth(string str)
    {
        int width = 0;
        foreach (char ch in str)
        {
            // CJK 한글, 한자, 전각 기호 등은 2칸 차지
            if (char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.OtherLetter ||
                (ch >= 0x1100 && ch <= 0x11FF) || // Hangul Jamo
                (ch >= 0xAC00 && ch <= 0xD7A3))   // Hangul Syllables
            {
                width += 2;
            }
            else
            {
                width += 1;
            }
        }
        return width;
    }

    /// <summary>
    /// 시각적 너비 기준에 맞춰 문자열을 자릅니다.
    /// </summary>
    private static string TruncateToDisplayWidth(string str, int maxWidth)
    {
        if (GetDisplayWidth(str) <= maxWidth)
            return str;

        string ellipsis = "...";
        int ellipsisWidth = 3;
        int allowedWidth = maxWidth - ellipsisWidth;

        if (allowedWidth <= 0)
            return ellipsis;

        // 뒤쪽(파일명 위주)을 남기기 위해 역순 계산
        int currentWidth = 0;
        int startIndex = str.Length;

        for (int i = str.Length - 1; i >= 0; i--)
        {
            int charWidth = GetDisplayWidth(str[i].ToString());
            if (currentWidth + charWidth > allowedWidth)
                break;

            currentWidth += charWidth;
            startIndex = i;
        }

        return ellipsis + str[startIndex..];
    }
}