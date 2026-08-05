using Silk.NET.Maths;

namespace YumeArisu.Core.Rendering;

public class Screen
{
    public Vector2D<int> Resolution { get; set; }
    public float AspectRatio => (float)Resolution.X / Resolution.Y;
    public bool IsFixedResolution { get; set; }

}