using System.Numerics;
using YumeArisu.Core.Internal.RenderPipeline;

namespace YumeArisu.Core.Rendering;

public class MaterialOverride
{
    private readonly Dictionary<string, ShaderPropertyValue> _properties = new();
    private readonly Dictionary<string, Texture> _textures = new();

    public Texture GetTexture(string name) => _textures.TryGetValue(name, out var value) ? value : throw new Exception("해당하는 Texture가 존재하지 않습니다.");

    public int GetInt(string name) => GetValue(name, ShaderPropertyType.Int).AsInt();
    public float GetFloat(string name) => GetValue(name, ShaderPropertyType.Float).AsFloat();
    public Vector2 GetVector2(string name) => GetValue(name, ShaderPropertyType.Vec2).AsVector2();
    public Vector3 GetVector3(string name) => GetValue(name, ShaderPropertyType.Vec3).AsVector3();
    public Vector4 GetVector4(string name) => GetValue(name, ShaderPropertyType.Vec4).AsVector4();
    public Matrix4x4 GetMatrix4x4(string name) => GetValue(name, ShaderPropertyType.Mat4).AsMatrix4x4();

    public void SetTexture(string name, Texture texture) => _textures[name] = texture;
    public void SetInt(string name, int value) => SetShaderProperty(name, ShaderPropertyValue.FromInt(value));
    public void SetFloat(string name, float value) => SetShaderProperty(name, ShaderPropertyValue.FromFloat(value));
    public void SetVector2(string name, Vector2 value) => SetShaderProperty(name, ShaderPropertyValue.FromVector2(value));
    public void SetVector3(string name, Vector3 value) => SetShaderProperty(name, ShaderPropertyValue.FromVector3(value));
    public void SetVector4(string name, Vector4 value) => SetShaderProperty(name, ShaderPropertyValue.FromVector4(value));
    public void SetMatrix4x4(string name, Matrix4x4 value) => SetShaderProperty(name, ShaderPropertyValue.FromMatrix4x4(value));

    public void Clear()
    {
        _properties.Clear();
        _textures.Clear();
    }

    internal IReadOnlyDictionary<string, ShaderPropertyValue> Properties => _properties;
    internal IReadOnlyDictionary<string, Texture> Textures => _textures;

    internal void SetShaderProperty(string name, ShaderPropertyValue value) => _properties[name] = value;

    private ShaderPropertyValue GetValue(string name, ShaderPropertyType expectedType)
    {
        if (!_properties.TryGetValue(name, out var value))
            throw new Exception("해당하는 ShaderProperty가 존재하지 않습니다.");

        if (value.Type != expectedType)
            throw new Exception("해당하는 ShaderProperty의 타입이 일치하지 않습니다.");

        return value;
    }
}