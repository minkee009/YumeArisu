using System.Runtime.CompilerServices;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class ApplicationSystem : SystemBase<ApplicationSystem, IApplication>
{
    private IApplication _application;

    internal override void StartUpInternal(IApplication application) 
    {
        _application = application;
    }

    internal override void ShutDownInternal() => _application = null;

    public bool IsRunning() => _application.IsRunning();

    public void RequestClose() => _application.RequestClose();

    public void SetTargetFrameRate(int fps) => _application.SetTargetFrameRate(fps);
}

// 문법 설탕용 클래스
public static class Application
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRunning() => ApplicationSystem.Instance.IsRunning();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RequestClose() => ApplicationSystem.Instance.RequestClose();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTargetFrameRate(int fps) => ApplicationSystem.Instance.SetTargetFrameRate(fps);
}