namespace YumeArisu.Core.Abstractions;

/// <summary>
/// 어플리케이션의 생명주기 관리 및 루프 제어 설정을 제공합니다.
/// </summary>
public interface IApplication
{
    public bool IsRunning();
    public void RequestClose();
    public void SetTargetFrameRate(int fps);
    public void SetVSync(bool enabled);     // TODO : 렌더링 옵션에서 VSync를 껐다 켤 수 있는 방향으로 가기 -> RenderSettings 이런걸로
}

