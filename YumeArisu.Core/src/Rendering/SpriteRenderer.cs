using System.Numerics;
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

    private readonly ShaderPropertyBlock _materialOverride = new();
    private readonly ShaderPropertyBlock _objectProperties = new();
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

        UpdateSpriteData(
            out var model,
            out var size,
            out var pivot,
            out var uvRect,
            out var color);

        if (Material.Shader == BuiltInRenderResources.DefaultSpriteShader)
        {
            Batcher.Submit(
                Sprite.Texture,
                model,
                size,
                pivot,
                uvRect,
                color,
                Material.BlendMode,
                ViewSpaceDepth);
            return;
        }

        UpdateCustomObjectProperties(model, size, pivot, uvRect, color);

        var command = new RenderCommand
        {
            Mesh = BuiltInRenderResources.DefaultQuadMesh,
            Material = Material,
            MaterialOverride = _materialOverride,
            ObjectProperties = _objectProperties,
            Depth = ViewSpaceDepth
        };

        context.Submit(command);
    }

    private void UpdateSpriteData(
        out Matrix4x4 model,
        out Vector2 size,
        out Vector2 pivot,
        out Vector4 uvRect,
        out Vector4 color)
    {
        var texture = Sprite.Texture;
        var rect = Sprite.Rect;
        pivot = Sprite.Pivot;

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

        size = new(rect.Size.X / Sprite.PPU, rect.Size.Y / Sprite.PPU);
        uvRect = new(u0, v0, uw, vh);
        color = _color.ToVector4();
        model = Transform.WorldMatrix;
    }

    private void UpdateCustomObjectProperties(
        Matrix4x4 model,
        Vector2 size,
        Vector2 pivot,
        Vector4 uvRect,
        Vector4 color)
    {
        if (!_objectPropertiesDirty)
        {
            _objectProperties.SetMatrix4x4("Model", model);
            return;
        }

        _objectProperties.SetMatrix4x4("Model", model);
        _objectProperties.SetVector2("SpriteSize", size);
        _objectProperties.SetVector2("SpritePivot", pivot);
        _objectProperties.SetVector4("UVRect", uvRect);
        _objectProperties.SetVector4("Color", color);
        _objectProperties.SetTexture("MainTexture", Sprite.Texture);
        _objectPropertiesDirty = false;
    }
}