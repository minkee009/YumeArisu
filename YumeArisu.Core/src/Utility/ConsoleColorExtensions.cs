namespace YumeAris.Core.Utility;

public static class ConsoleColorExtensions
{
    public static void WriteLineColored(string message, ConsoleColor color)
    {
        string code = color switch
        {
            ConsoleColor.Red => "\u001b[31m",
            ConsoleColor.Green => "\u001b[32m",
            ConsoleColor.Yellow => "\u001b[33m",
            ConsoleColor.Blue => "\u001b[34m",
            ConsoleColor.Magenta => "\u001b[35m",
            ConsoleColor.Cyan => "\u001b[36m",
            _ => "\u001b[37m"
        };

        Console.WriteLine($"{code}{message}\u001b[0m");
    }
}