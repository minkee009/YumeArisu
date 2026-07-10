using YumeArisu.Desktop.Implements;

namespace YumeArisu.Desktop;

internal class Program
{
    private static DesktopApplication _app;

    public static void Main()
    {
        _app = new("夢を見るアリス", 640, 480);
        _app.Run();
    }
}
