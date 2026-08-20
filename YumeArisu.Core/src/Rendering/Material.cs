using System.Text;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Utility;
using YumeArisu.Core.Internal.ResourceHandling;
using YumeArisu.Core.Internal.RenderPipeline;
using System.Numerics;

namespace YumeArisu.Core.Rendering;

public class Material : Resource
{
    public Shader Shader { get; private set; }
    private Dictionary<string, Texture> _textures = new();
    private Dictionary<string, UniformValue> _uniforms = new();

    internal bool ImmediateLoadFromReference(Shader shader, 
        Dictionary<string, Texture> textures, 
        Dictionary<string, UniformValue> uniforms)
    {
        if (IsLoaded)
            return false;

        Shader = shader;
        _textures = textures;
        _uniforms = uniforms;

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

        _uniforms = meta.Uniforms;

        return true;
    }

    protected override void OnUnload()
    {
        if(Shader.IsLoadedBySystem())
            Resources.Release(Shader);

        foreach (var tex in _textures.Values)
        {
            if(tex.IsLoadedBySystem())
                Resources.Release(tex);
        }
            
        _textures.Clear();
        _uniforms = null;
        Shader = null;
    }

    public Texture GetTexture(string name) => _textures.TryGetValue(name, out var value) ? value : throw new Exception("해당하는 Texture가 존재하지 않습니다.");
    public int GetInt(string name)
    {
        if (_uniforms.TryGetValue(name, out var value))
        {
            if (value.Type == UniformType.Int)
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
        if (_uniforms.TryGetValue(name, out var value))
        {
            if (value.Type == UniformType.Float)
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
        if (_uniforms.TryGetValue(name, out var value))
        {
            if (value.Type == UniformType.Vec2)
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
        if (_uniforms.TryGetValue(name, out var value))
        {
            if (value.Type == UniformType.Vec3)
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
        if (_uniforms.TryGetValue(name, out var value))
        {
            if (value.Type == UniformType.Vec4)
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
        if (_uniforms.TryGetValue(name, out var value))
        {
            if (value.Type == UniformType.Mat4)
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
    public void SetInt(string name, int value) => SetUniform(name, UniformValue.FromInt(value));
    public void SetFloat(string name, float value) => SetUniform(name, UniformValue.FromFloat(value));
    public void SetVector2(string name, Vector2 value) => SetUniform(name, UniformValue.FromVector2(value));
    public void SetVector3(string name, Vector3 value) => SetUniform(name, UniformValue.FromVector3(value));
    public void SetVector4(string name, Vector4 value) => SetUniform(name, UniformValue.FromVector4(value));
    public void SetMatrix4x4(string name, Matrix4x4 value) => SetUniform(name, UniformValue.FromMatrix4x4(value));

    internal void SetUniform(string name, UniformValue value) => _uniforms[name] = value;

    public void Apply(MaterialPropertyOverride overrides = null)
    {
        Shader.Use();

        // 이름 기준으로 override가 있으면 override 값 우선 사용
        foreach (var (name, value) in _uniforms)
        {
            var actual = overrides != null && overrides.UniformOverrides.TryGetValue(name, out var ov)
                ? ov
                : value;
            Shader.SetUniform(name, actual);
        }

        int unit = 0;
        foreach (var (name, tex) in _textures)
        {
            var actual = overrides != null && overrides.TextureOverrides.TryGetValue(name, out var ov)
                ? ov
                : tex;
            Shader.SetTexture(name, actual, unit++);
        }

        if (overrides == null)
            return;

        // base Material에 없던 완전히 새로운 uniform/텍스쳐 이름만 추가로 처리
        foreach (var (name, value) in overrides.UniformOverrides)
        {
            if (!_uniforms.ContainsKey(name))
                Shader.SetUniform(name, value);
        }

        foreach (var (name, tex) in overrides.TextureOverrides)
        {
            if (!_textures.ContainsKey(name))
                Shader.SetTexture(name, tex, unit++);
        }
    }
}