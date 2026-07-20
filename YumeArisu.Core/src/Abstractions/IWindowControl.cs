using Silk.NET.Maths;

namespace YumeArisu.Core.Abstractions;

/// <summary>
/// 윈도우 핸들(혹은 뷰)의 기본 제어를 제공합니다.
/// </summary>
public interface IWindowControl
{
    public void SetScreenMode(ScreenMode screenMode);
    public void SetTitle(string title);
    public void SetSize(int width, int height);
    public void SetPosition(int x, int y);

    public ScreenMode GetScreenMode();
    public string GetTitle();
    public Vector2D<int> GetSize();
    public Vector2D<int> GetPosition();
}