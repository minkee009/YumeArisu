using System.Runtime.CompilerServices;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class ApplicationSystem : SystemBase<ApplicationSystem, IApplicationControl>
{
    private IApplicationControl _control;

    internal override void StartUpInternal(IApplicationControl control) 
    {
        _control = control;
    }
    internal override void ShutDownInternal() => _control = null;

    public int TargetFrameRate { get => _control.TargetFrameRate; set => _control.TargetFrameRate = value; }
    public bool VSync { get => _control.VSync; set => _control.VSync = value; }
    public bool IsRunning() => _control.IsRunning();
    public void RequestClose() => _control.RequestClose();
}

// 문법 설탕용 클래스
public static class ApplicationControl
{
    public static int TargetFrameRate { get => ApplicationSystem.Instance.TargetFrameRate; set => ApplicationSystem.Instance.TargetFrameRate = value; }
    
    public static bool VSync { get => ApplicationSystem.Instance.VSync; set => ApplicationSystem.Instance.VSync = value; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRunning() => ApplicationSystem.Instance.IsRunning();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RequestClose() => ApplicationSystem.Instance.RequestClose();
}