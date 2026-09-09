using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Common;

namespace YumeArisu.Core.Rendering;

public class SpriteRenderer : Renderer
{
    public Sprite Sprite 
    { 
        get => _sprite; 
        set
        {
            if (_sprite == value)
                return;

            _sprite = value;
            _objectPropertiesDirty = true;
        } 
    }

    public Color Color 
    { 
        get => _color; 
        set
        {
            _color = value;
            _objectPropertiesDirty = true;
        } 
    }

    public bool FlipX 
    { 
        get => _flipX; 
        set
        {
            if (_flipX == value)
                return;

            _flipX = value;
            _objectPropertiesDirty = true;
        } 
    }

    public bool FlipY
    { 
        get => _flipY; 
        set
        {
            if (_flipY == value)
                return;

            _flipY = value;
            _objectPropertiesDirty = true;
        } 
    }

    internal SpriteBatcher Batcher { get; set; }
    internal bool UsesImmediateDraw => Material?.Shader != BuiltInRenderResources.DefaultSpriteShader;

    private bool _objectPropertiesDirty = true;
    private Sprite _sprite;
    private Color _color;
    private bool _flipX;
    private bool _flipY;

    public SpriteRenderer()
    {
        Material = BuiltInRenderResources.DefaultSpriteMaterial; 
        Color = Color.White;
    }

    protected internal override void OnAttach()
    {
        RenderSystem.Instance.RegisterSpriteRenderer(this);
    }

    protected internal override void OnDetach()
    {
        RenderSystem.Instance.UnregisterSpriteRenderer(this);
    }

    internal override void Draw(RenderContext context)
    {
        if (Sprite is null) 
            return;

        UpdateObjectProperties();

        var command = new RenderCommand
        {
            Mesh = BuiltInRenderResources.DefaultQuadMesh,
            Material = Material,
            MaterialOverride = MaterialOverride,
            ObjectProperties = ObjectProperties,
            Depth = ViewSpaceDepth
        };

        if (Material.Shader == BuiltInRenderResources.DefaultSpriteShader)
        {
            Batcher.Submit(command);
            return;
        }

        context.Submit(command);
    }

    private void UpdateObjectProperties()
    {
        if (!_objectPropertiesDirty)
        {
            ObjectProperties.SetMatrix4x4("Model", Transform.WorldMatrix);
            return;
        }

        var texture = Sprite.Texture;
        var rect = Sprite.Rect;
        var pivot = Sprite.Pivot;

        float u0 = rect.Origin.X / texture.Width;
        float v0 = rect.Origin.Y / texture.Height;
        float uw = rect.Size.X / texture.Width;
        float vh = rect.Size.Y / texture.Height;

        if (FlipX)
        {
            u0 += uw;
            uw = -uw;
        }
        if (FlipY)
        {
            v0 += vh;
            vh = -vh;
        }

        float sizeX = rect.Size.X / Sprite.PPU;
        float sizeY = rect.Size.Y / Sprite.PPU;

        ObjectProperties.SetMatrix4x4("Model", Transform.WorldMatrix);
        ObjectProperties.SetVector2("SpriteSize", new(sizeX, sizeY));
        ObjectProperties.SetVector2("SpritePivot", new(pivot.X, pivot.Y));
        ObjectProperties.SetVector4("UVRect", new(u0, v0, uw, vh));
        ObjectProperties.SetVector4("Color", _color.ToVector4());
        ObjectProperties.SetTexture("MainTexture", texture);

        _objectPropertiesDirty = false;
    }
}