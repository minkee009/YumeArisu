using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Common;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Internal.RenderPipeline;

public abstract class Renderer : Component
{
    public bool Enabled { get; set; }

    public int RenderOrder
    {
        get => _renderOrder;
        set
        {
            if (_renderOrder == value)
                return;

            _renderOrder = value;

            if (IsRegistered)
                RenderSystem.Instance.MarkRenderOrderDirty();
        }
    }

    public Material Material
    {
        get => _material;
        set
        {
            if (ReferenceEquals(_material, value))
                return;

            _material = value;

            if (IsRegistered)
                RenderSystem.Instance.MarkRenderOrderDirty();
        }
    }

    public BoundingBox Bounds { get; }

    internal long RegistrationOrder { get; set; }
    internal float ViewSpaceDepth { get; set; }
    internal bool IsRegistered { get => _isRegistered; set => _isRegistered = value; }

    private int _renderOrder;
    private bool _isRegistered;

    private Material _material;

    internal abstract void Draw(RenderContext context);
}