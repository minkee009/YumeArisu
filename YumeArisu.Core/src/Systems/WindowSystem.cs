using Silk.NET.Maths;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class WindowSystem : SystemBase<WindowSystem, IWindowControl>
{
    private IWindowControl _control;

    internal override void OnStartUp(IWindowControl control) => _control = control;

    internal override void OnShutDown() => _control = null;

    public DisplayMode DisplayMode { get => _control.DisplayMode; set => _control.DisplayMode = value; }
    public string Title { get => _control.Title; set => _control.Title = value; }
    public Vector2D<int> Size { get => _control.Size; set => _control.Size = value; }
    public Vector2D<int> Position { get => _control.Position; set => _control.Position = value; }
}

// 문법 설탕용 클래스
public static class WindowControl
{
    public static DisplayMode DisplayMode { get => WindowSystem.Instance.DisplayMode; set => WindowSystem.Instance.DisplayMode = value; }
    public static string Title { get => WindowSystem.Instance.Title; set => WindowSystem.Instance.Title = value; }
    public static Vector2D<int> Size { get => WindowSystem.Instance.Size; set => WindowSystem.Instance.Size = value; }
    public static Vector2D<int> Position { get => WindowSystem.Instance.Position; set => WindowSystem.Instance.Position = value; }
}