using System.Text;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Utility;
using YumeArisu.Core.Internal.ResourceHandling;
using YumeArisu.Core.Internal.RenderPipeline;
using System.Numerics;

namespace YumeArisu.Core.Rendering;

using EnableCap = Silk.NET.OpenGL.EnableCap;
using BlendingFactor = Silk.NET.OpenGL.BlendingFactor;

public class Material : Resource
{
    public Shader Shader { get; private set; }
    public BlendMode BlendMode { get; set; } = BlendMode.Opaque;
    public RenderQueue RenderQueue { get; set; } = RenderQueue.Geometry;
    private Dictionary<string, Texture> _textures = new();
    private ShaderPropertyBlock _shaderProperties = new();

    internal bool ImmediateLoadFromReference(Shader shader, 
        Dictionary<string, Texture> textures, 
        Dictionary<string, ShaderPropertyValue> shaderProperties,
        BlendMode blendMode = BlendMode.Opaque,
        RenderQueue renderQueue = RenderQueue.Geometry)
    {
        if (IsLoaded)
            return false;

        Shader = shader;
        _textures = textures;
        _shaderProperties = CreateShaderProperties(shaderProperties);
        BlendMode = blendMode;
        RenderQueue = renderQueue;

        IsLoaded = true;
        return true;
    }

    protected override bool OnLoad(byte[] bytes)
    {
        // .material 파일인지 확인
        if (!PathHelper.HasExtension(Path, ".material"))
            return false;

        // material meta로 전환
        var json = Encoding.UTF8.GetString(bytes);
        var meta = JsonMetaParser.Parse<MaterialMeta>(json);

        Shader = Resources.Get<Shader>(meta.ShaderPath);

        foreach (var (name, path) in meta.TexturePaths)
            _textures[name] = Resources.Get<Texture>(path);

        _shaderProperties = CreateShaderProperties(meta.ShaderProperties);
        BlendMode = meta.BlendMode ?? BlendMode.Opaque;
        RenderQueue = meta.RenderQueue ?? Rendering.RenderQueue.Geometry;

        return true;
    }

    protected override void OnUnload()
    {
        if (Shader.IsLoadedBySystem)
            Resources.Release(Shader);

        foreach (var tex in _textures.Values)
        {
            if (tex.IsLoadedBySystem)
                Resources.Release(tex);
        }
            
        _textures.Clear();
        _shaderProperties = null;
        Shader = null;
    }

    public Texture GetTexture(string name) => _textures.TryGetValue(name, out var value) ? value : throw new Exception("해당하는 Texture가 존재하지 않습니다.");
    public int GetInt(string name)
    {
        if (_shaderProperties.Properties.TryGetValue(name, out var value))
        {
            if (value.Type == ShaderPropertyType.Int)
                return value.AsInt();
            else
                throw new Exception("해당하는 값은 Int타입이 아닙니다.");
        }
        else
        {
            throw new Exception("해당하는 값이 존재하지 않습니다.");
        }
    }

    public float GetFloat(string name)
    {
        if (_shaderProperties.Properties.TryGetValue(name, out var value))
        {
            if (value.Type == ShaderPropertyType.Float)
                return value.AsFloat();
            else
                throw new Exception("해당하는 값은 Float타입이 아닙니다.");
        }
        else
        {
            throw new Exception("해당하는 값이 존재하지 않습니다.");
        }
    }

    public Vector2 GetVector2(string name)
    {
        if (_shaderProperties.Properties.TryGetValue(name, out var value))
        {
            if (value.Type == ShaderPropertyType.Vec2)
                return value.AsVector2();
            else
                throw new Exception("해당하는 값은 Vector2타입이 아닙니다.");
        }
        else
        {
            throw new Exception("해당하는 값이 존재하지 않습니다.");
        }
    }

    public Vector3 GetVector3(string name)
    {
        if (_shaderProperties.Properties.TryGetValue(name, out var value))
        {
            if (value.Type == ShaderPropertyType.Vec3)
                return value.AsVector3();
            else
                throw new Exception("해당하는 값은 Vector3타입이 아닙니다.");
        }
        else
        {
            throw new Exception("해당하는 값이 존재하지 않습니다.");
        }
    }

    public Vector4 GetVector4(string name)
    {
        if (_shaderProperties.Properties.TryGetValue(name, out var value))
        {
            if (value.Type == ShaderPropertyType.Vec4)
                return value.AsVector4();
            else
                throw new Exception("해당하는 값은 Vector4타입이 아닙니다.");
        }
        else
        {
            throw new Exception("해당하는 값이 존재하지 않습니다.");
        }
    }

    public Matrix4x4 GetMatrix4x4(string name)
    {
        if (_shaderProperties.Properties.TryGetValue(name, out var value))
        {
            if (value.Type == ShaderPropertyType.Mat4)
                return value.AsMatrix4x4();
            else
                throw new Exception("해당하는 값은 Matrix4x4타입이 아닙니다.");
        }
        else
        {
            throw new Exception("해당하는 값이 존재하지 않습니다.");
        }
    }

    public void SetTexture(string name, Texture texture) => _textures[name] = texture;
    public void SetInt(string name, int value) => SetShaderProperty(name, ShaderPropertyValue.FromInt(value));
    public void SetFloat(string name, float value) => SetShaderProperty(name, ShaderPropertyValue.FromFloat(value));
    public void SetVector2(string name, Vector2 value) => SetShaderProperty(name, ShaderPropertyValue.FromVector2(value));
    public void SetVector3(string name, Vector3 value) => SetShaderProperty(name, ShaderPropertyValue.FromVector3(value));
    public void SetVector4(string name, Vector4 value) => SetShaderProperty(name, ShaderPropertyValue.FromVector4(value));
    public void SetMatrix4x4(string name, Matrix4x4 value) => SetShaderProperty(name, ShaderPropertyValue.FromMatrix4x4(value));

    internal void SetShaderProperty(string name, ShaderPropertyValue value) => _shaderProperties.SetShaderProperty(name, value);

    internal void ApplyRenderState()
        => ApplyRenderState(BlendMode);

    internal void ApplyRenderState(BlendMode blendMode)
    {
        var gl = RenderSystem.Instance.GetGL();

        switch (blendMode)
        {
            case BlendMode.Opaque:
                gl.Disable(EnableCap.Blend);
                gl.DepthMask(true);
                break;
            case BlendMode.Alpha:
                gl.Enable(EnableCap.Blend);
                gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                gl.DepthMask(false);
                break;
            case BlendMode.Additive:
                gl.Enable(EnableCap.Blend);
                gl.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                gl.DepthMask(false);
                break;
        }
    }

    public void Apply(ShaderPropertyBlock overrides = null)
    {
        Shader.Use();

        // 이름 기준으로 override가 있으면 override 값 우선 사용
        foreach (var (name, value) in _shaderProperties.Properties)
        {
            var actual = overrides is not null && overrides.Properties.TryGetValue(name, out var ov)
                ? ov
                : value;
            Shader.SetShaderProperty(name, actual);
        }

        int unit = 0;
        foreach (var (name, tex) in _textures)
        {
            var actual = overrides is not null && overrides.Textures.TryGetValue(name, out var ov)
                ? ov
                : tex;
            Shader.SetTexture(name, actual, unit++);
        }

        if (overrides is null)
            return;

        // base Material에 없던 새로운 ShaderProperty/텍스쳐 이름만 추가로 처리
        foreach (var (name, value) in overrides.Properties)
        {
            if (!_shaderProperties.Properties.ContainsKey(name))
                Shader.SetShaderProperty(name, value);
        }

        foreach (var (name, tex) in overrides.Textures)
        {
            if (!_textures.ContainsKey(name))
                Shader.SetTexture(name, tex, unit++);
        }
    }

    private static ShaderPropertyBlock CreateShaderProperties(Dictionary<string, ShaderPropertyValue> values)
    {
        var properties = new ShaderPropertyBlock();
        foreach (var (name, value) in values)
            properties.SetShaderProperty(name, value);
        return properties;
    }
}