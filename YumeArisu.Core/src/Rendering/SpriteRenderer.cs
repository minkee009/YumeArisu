using Silk.NET.OpenGL;
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
            _materialPropertyDirty = true;
        } 
    }

    public Color Color 
    { 
        get => _color; 
        set
        {
            _color = value;
            _materialPropertyDirty = true;
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
            _materialPropertyDirty = true;
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
            _materialPropertyDirty = true;
        } 
    }

    public MaterialPropertyOverride MaterialOverride => _materialOverride;

    internal SpriteBatcher Batcher { get; set; }

    private readonly MaterialPropertyOverride _materialOverride = new();
    private bool _materialPropertyDirty = true;
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

        UpdateMaterialProperties();

        if (Material.Shader == BuiltInRenderResources.DefaultSpriteShader)
        {
            Batcher.Submit(
                Sprite.Texture,
                Transform.WorldMatrix,
                MaterialOverride.GetVector2("SpriteSize"),
                MaterialOverride.GetVector2("SpritePivot"),
                MaterialOverride.GetVector4("UVRect"),
                MaterialOverride.GetVector4("Color"));
            return;
        }

        DrawImmediate();
    }

    private void DrawImmediate()
    {
        if (Sprite is null)
            return;

        UpdateMaterialProperties();

        Material.Apply(_materialOverride);
        Material.Shader.SetMatrix4x4("Model", Transform.WorldMatrix);

        var gl = RenderSystem.Instance.GetGL();

        var quad = BuiltInRenderResources.DefaultQuadMesh;

        gl.BindVertexArray(quad.VAOHandle);
        
        unsafe
        {
            gl.DrawElements(GLEnum.Triangles, quad.IndexCount, DrawElementsType.UnsignedInt, null);

#if DEBUG
            var err = gl.GetError();
            if (err != GLEnum.NoError)
                Console.WriteLine($"GL Error: {err}");
#endif
        }

        gl.BindVertexArray(0);
    }

    private void UpdateMaterialProperties()
    {
        if (!_materialPropertyDirty) return;

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

        MaterialOverride.SetVector2("SpriteSize", new(sizeX, sizeY));
        MaterialOverride.SetVector2("SpritePivot", new(pivot.X, pivot.Y));
        MaterialOverride.SetVector4("UVRect", new(u0, v0, uw, vh));
        MaterialOverride.SetTexture("MainTexture", texture);
        MaterialOverride.SetFloat("FlipX", FlipX ? 1 : 0);
        MaterialOverride.SetFloat("FlipY", FlipY ? 1 : 0);
        MaterialOverride.SetVector4("Color", _color.ToVector4());

        _materialPropertyDirty = false;
    }
}