using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Common;

namespace YumeArisu.Core.Internal.RenderPipeline;

public abstract class Renderer : Component
{
    public bool Enabled { get; set; }

    public int RenderOrder { get; set; }

    public Material Material { get; set; }

    public BoundingBox Bounds { get; }

    internal abstract void Draw();
}