using YumeArisu.Desktop.Implements;

namespace YumeArisu.Desktop;

internal class Program
{
    private static DesktopApplication _app;

    public static void Main()
    {
        _app = new("夢を見るアリス", 1280, 720);
        _app.Run();
    }
}
