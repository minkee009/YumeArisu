using System.Runtime.CompilerServices;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class WindowSystem : SystemBase<WindowSystem, IWindowControl>
{
    private IWindowControl _control;

    internal override void StartUpInternal(IWindowControl control) 
    {
        _control = control;
    }

    internal override void ShutDownInternal() => _control = null;

    public void SetScreenMode(ScreenMode screenMode) => _control.SetScreenMode(screenMode);
    public void SetTitle(string title) => _control.SetTitle(title);
    public void SetSize(int width, int height) => _control.SetSize(width, height);
    public void SetPosition(int x, int y) => _control.SetPosition(x, y);
}

// 문법 설탕용 클래스
public static class WindowControl
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetScreenMode(ScreenMode screenMode) => WindowSystem.Instance.SetScreenMode(screenMode);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTitle(string title) => WindowSystem.Instance.SetTitle(title);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetSize(int width, int height)  => WindowSystem.Instance.SetSize(width, height);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPosition(int x, int y) => WindowSystem.Instance.SetPosition(x, y);
}