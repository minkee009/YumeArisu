namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct TilemapLayoutMeta
{
    public string TilesetPath { get; init; }
    public int OriginX { get; init; }
    public int OriginY { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int[] Tiles { get; init; } // row-major, 0 = 빈 칸
}