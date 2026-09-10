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

    public MaterialOverride MaterialOverride => _materialOverride;

    internal SpriteBatcher Batcher { get; set; }
    internal bool UsesImmediateDraw => Material?.Shader != BuiltInRenderResources.DefaultSpriteShader;

    private readonly MaterialOverride _materialOverride = new();
    private SpriteObjectData _objectData;
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

    internal override void Draw()
    {
        if (Sprite is null) 
            return;

        UpdateSpriteData(
            out var model,
            out var size,
            out var pivot,
            out var uvRect,
            out var color);

        if (!UsesImmediateDraw)
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

        UpdateObjectData(model, size, pivot, uvRect, color);

        // per-material(Material 자체 + MaterialOverride) 적용 -> 다음 여유 텍스쳐 유닛을 돌려받음
        Material.ApplyRenderState();
        int nextTextureUnit = Material.Apply(_materialOverride);

        // per-object 적용은 완전히 별도 경로 - 이름 dictionary 병합이 아니라 고정 필드 업로드
        _objectData.Apply(Material.Shader, nextTextureUnit);

        BuiltInRenderResources.DefaultQuadMesh.DrawElements();
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

    private void UpdateObjectData(
        Matrix4x4 model,
        Vector2 size,
        Vector2 pivot,
        Vector4 uvRect,
        Vector4 color)
    {
        _objectData.Model = model;

        if (!_objectPropertiesDirty)
            return;

        _objectData.SpriteSize = size;
        _objectData.SpritePivot = pivot;
        _objectData.UVRect = uvRect;
        _objectData.Color = color;
        _objectData.MainTexture = Sprite.Texture;
        _objectPropertiesDirty = false;
    }
}