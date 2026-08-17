using System.Numerics;
using YumeArisu.Core.Internal.RenderPipeline;

namespace YumeArisu.Core.Rendering;

public class MaterialPropertyOverride
{
    private Dictionary<string, UniformValue> _uniformOverrides = new();
    private Dictionary<string, Texture> _textureOverrides = new();


    public Texture GetTexture(string name) => _textureOverrides.TryGetValue(name, out var value) ? value : throw new Exception("해당하는 Texture가 존재하지 않습니다.");
    public int GetInt(string name)
    {
        if (_uniformOverrides.TryGetValue(name, out var value))
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
        if (_uniformOverrides.TryGetValue(name, out var value))
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
        if (_uniformOverrides.TryGetValue(name, out var value))
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
        if (_uniformOverrides.TryGetValue(name, out var value))
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
        if (_uniformOverrides.TryGetValue(name, out var value))
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

    public Matrix4x4 GetMatrix4(string name)
    {
        if (_uniformOverrides.TryGetValue(name, out var value))
        {
            if (value.Type == UniformType.Mat4)
                return value.AsMatrix4x4();
            else
                throw new Exception("해당하는 값은 Matrix4타입이 아닙니다.");
        }
        else
        {
            throw new Exception("해당하는 값이 존재하지 않습니다.");
        }
    }

    public void SetTexture(string name, Texture texture) => _textureOverrides[name] = texture;
    public void SetInt(string name, int value) => SetUniform(name, UniformValue.FromInt(value));
    public void SetFloat(string name, float value) => SetUniform(name, UniformValue.FromFloat(value));
    public void SetVector2(string name, Vector2 value) => SetUniform(name, UniformValue.FromVector2(value));
    public void SetVector3(string name, Vector3 value) => SetUniform(name, UniformValue.FromVector3(value));
    public void SetVector4(string name, Vector4 value) => SetUniform(name, UniformValue.FromVector4(value));
    public void SetMatrix4x4(string name, Matrix4x4 value) => SetUniform(name, UniformValue.FromMatrix4x4(value));

    internal void SetUniform(string name, UniformValue value) => _uniformOverrides[name] = value;

    public void Clear()
    {
        _uniformOverrides.Clear();
        _textureOverrides.Clear();
    }

    internal IReadOnlyDictionary<string, Texture> TextureOverrides => _textureOverrides;
    internal IReadOnlyDictionary<string, UniformValue> UniformOverrides => _uniformOverrides;
}