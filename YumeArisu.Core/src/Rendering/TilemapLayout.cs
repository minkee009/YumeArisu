using System.Text;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Utility;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Rendering;

/// <summary>
/// 타일 배치를 담은 재사용 가능한 리소스입니다. TilemapRenderer.SetTileFromLayout으로
/// 렌더러의 그리드에 스탬프되며, 대입 이후로는 원본과 독립적입니다.
/// </summary>
public class TilemapLayout : Resource
{
    internal Tileset Tileset { get; private set; }
    internal int OriginX { get; private set; }
    internal int OriginY { get; private set; }
    internal int Width { get; private set; }
    internal int Height { get; private set; }
    internal int[] Tiles { get; private set; }

    protected override bool OnLoad(byte[] bytes)
    {
        if (!PathHelper.HasExtension(Path, ".tilemaplayout"))
            return false;

        var json = Encoding.UTF8.GetString(bytes);
        var meta = JsonMetaParser.Parse<TilemapLayoutMeta>(json);

        if (!string.IsNullOrEmpty(meta.TilesetPath))
            Tileset = Resources.Get<Tileset>(meta.TilesetPath);

        OriginX = meta.OriginX;
        OriginY = meta.OriginY;
        Width = meta.Width;
        Height = meta.Height;
        Tiles = meta.Tiles ?? Array.Empty<int>();

        return true;
    }

    protected override void OnUnload()
    {
        if (IsLoadedBySystem)
        {
            if (Tileset is not null)
                Resources.Release(Tileset);
        }

        Tileset = null;
        Tiles = null;
        Width = Height = 0;
    }
}