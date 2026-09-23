using System.Numerics;
using System.Text;
using YumeArisu.Core.Utility;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Rendering;

public class Tileset : Resource
{
    internal Texture Texture { get; private set; }
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }
    
    /// <summary>계산 불가 상태(텍스처 없음 또는 TileWidth 미확정)면 -1을 반환합니다.</summary>
    public int Columns => (Texture is not null && TileWidth > 0) ? Texture.Width / TileWidth : -1;

    /// <summary>계산 불가 상태면 -1을 반환합니다.</summary>
    public int Rows => (Texture is not null && TileHeight > 0) ? Texture.Height / TileHeight : -1;


    internal bool ImmediateLoadFromReference(Texture texture, int tileWidth, int tileHeight)
    {
        if (IsLoaded)
            return false;

        Texture = texture;
        TileWidth = tileWidth;
        TileHeight = tileHeight;

        IsLoaded = true;
        return true;
    }

    /// <summary>tileId(1부터)의 UV 사각형. (uMin, vMin, uMax, vMax)</summary>
    internal bool GetUV(int tileId, out Vector4 uv)
    {
        int columns = Columns;
        int rows = Rows;

        if (columns <= 0 || rows <= 0 || tileId < 1 || tileId > columns * rows)
        {
            uv = default;
            return false;
        }

        int index = tileId - 1;
        int col = index % columns;
        int row = index / columns; // 0행 = 이미지 맨 위

        float w = Texture.Width;
        float h = Texture.Height;
        const float inset = 0.5f; // 인접 타일 번짐 방지용 반 텍셀 인셋

        // Texture는 로드 시 수직 반전되므로 v는 아래→위로 증가
        uv = new Vector4(
            (col * TileWidth + inset) / w,
            1f - ((row + 1) * TileHeight - inset) / h,
            ((col + 1) * TileWidth - inset) / w,
            1f - (row * TileHeight + inset) / h);
        return true;
    }

    protected override bool OnLoad(byte[] bytes)
    {
        // .tileset 파일인지 확인
        if (!PathHelper.HasExtension(Path, ".tileset"))
            return false;

        // tileset meta로 전환
        var json = Encoding.UTF8.GetString(bytes);
        var meta = JsonMetaParser.Parse<TilesetMeta>(json);

        Texture = Resources.Get<Texture>(meta.TexturePath);
        TileWidth = meta.TileWidth;
        TileHeight = meta.TileHeight;

        return true;
    }


    protected override void OnUnload()
    {
        if (Texture.IsLoadedBySystem)
            Resources.Release(Texture);

        Texture = null;
        TileWidth = 0;
        TileHeight = 0;
    }
}