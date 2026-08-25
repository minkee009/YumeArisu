using System.Text;
using System.Numerics;
using Silk.NET.Maths;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Utility;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Rendering;

public class Sprite : Resource
{
    internal Texture Texture { get; private set; }
    public Vector2 Pivot { get; set; }    
    public Rectangle<float> Rect { get; set; }
    public float PPU { get; set; }

    // TODO : test 이후 internal로 전환
    public bool ImmediateLoadFromReference(Texture texture, Vector2 pivot, Rectangle<float> rect, float ppu = 100)
    {
        if (IsLoaded)
            return false;

        Texture = texture;
        if(texture.IsLoadedBySystem)
            Resources.Get<Texture>(texture.Path); // 캐시 카운트 증가

        Pivot = pivot;
        Rect = rect;
        PPU = ppu;

        IsLoaded = true;
        return true;
    }

    protected override bool OnLoad(byte[] bytes)
    {
        // .sprite 파일인지 확인
        if (!PathHelper.HasExtension(Path, ".sprite"))
            return false;

        // sprite meta로 전환
        var json = Encoding.UTF8.GetString(bytes);
        var meta = JsonMetaParser.Parse<SpriteMeta>(json);

        Texture = Resources.Get<Texture>(meta.TexturePath);

        Pivot = meta.Pivot; 
        Rect = meta.Rect;
        PPU = meta.PPU;

        return true;
    }

    protected override void OnUnload()
    {
        if(Texture.IsLoadedBySystem)
            Resources.Release(Texture);
        
        Texture = null;

        Pivot = default;
        Rect = default;
        PPU = 0.0f;
    }
}