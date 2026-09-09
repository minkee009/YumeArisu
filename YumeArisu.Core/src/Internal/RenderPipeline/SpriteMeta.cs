using System.Numerics;
using Silk.NET.Maths;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct SpriteMeta
{
    public string TexturePath { get; init; }
    public Vector2D<float> Pivot { get; init; }
    public Rectangle<float> Rect { get; init; }
    public float PPU { get; init; }
}