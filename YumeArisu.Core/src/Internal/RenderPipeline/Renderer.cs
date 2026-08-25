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

    protected internal override void OnAttach()
    {
        RenderSystem.Instance.RegisterRenderer(this);
    }

    protected internal override void OnDetach()
    {
        RenderSystem.Instance.UnregisterRenderer(this);
    }
}