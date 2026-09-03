using Silk.NET.Maths;
using YumeArisu.Core.Common;

namespace YumeArisu.Core.Abstractions;

/// <summary>
/// 윈도우 핸들(혹은 뷰)의 기본 제어를 제공합니다.
/// </summary>
public interface IWindowControl
{
    public DisplayMode DisplayMode { get; set; }
    public string Title { get; set; }
    public Vector2D<int> Size { get; set; }
    public Vector2D<int> Position { get; set; }
}