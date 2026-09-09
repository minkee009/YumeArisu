using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Common;

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
        }
    }

    public Material Material { get; set; }

    public BoundingBox Bounds { get; }

    internal ShaderPropertyBlock MaterialOverride { get; } = new();
    internal ShaderPropertyBlock ObjectProperties { get; } = new();

    internal long RegistrationOrder { get; set; }
    internal float ViewSpaceDepth { get; set; }
    internal bool IsRegistered { get => _isRegistered; set => _isRegistered = value; }

    private int _renderOrder;
    private bool _isRegistered;

    internal abstract void Draw(RenderContext context);
}