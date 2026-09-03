using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Internal.RenderPipeline;

public abstract class Renderer : Component
{
    public bool Enabled { get; set; }

    public int RenderOrder { get; set; }

    public Material Material { get; set; }

    internal abstract void Draw();
}