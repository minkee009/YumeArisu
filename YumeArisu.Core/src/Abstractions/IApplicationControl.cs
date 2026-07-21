namespace YumeArisu.Core.Abstractions;

/// <summary>
/// 어플리케이션의 생명주기 관리 및 루프 제어 설정을 제공합니다.
/// </summary>
public interface IApplicationControl
{
    public int TargetFrameRate { get; set; }
    public bool VSync { get; set; } 
    public bool IsRunning();
    public void RequestClose();
}

