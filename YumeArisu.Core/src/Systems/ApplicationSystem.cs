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

    public int TargetFrameRate { get => _application.TargetFrameRate; set => _application.TargetFrameRate = value; }
    public bool VSync { get => _application.VSync; set => _application.VSync = value; }
    public bool IsRunning() => _application.IsRunning();
    public void RequestClose() => _application.RequestClose();
}

// 문법 설탕용 클래스
public static class Application
{
    public static int TargetFrameRate { get => ApplicationSystem.Instance.TargetFrameRate; set => ApplicationSystem.Instance.TargetFrameRate = value; }
    
    public static bool VSync { get => ApplicationSystem.Instance.VSync; set => ApplicationSystem.Instance.VSync = value; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRunning() => ApplicationSystem.Instance.IsRunning();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RequestClose() => ApplicationSystem.Instance.RequestClose();
}