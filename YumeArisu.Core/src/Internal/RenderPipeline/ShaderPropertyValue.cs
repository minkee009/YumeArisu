using System.Numerics;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal struct ShaderPropertyValue
{
    public ShaderPropertyType Type { get; init; }
    public Vector4 Value { get; set; }
    public Matrix4x4 Matrix { get; set; }

    internal static ShaderPropertyValue FromInt(int v) => new() { Type = ShaderPropertyType.Int, Value = new(v, 0f, 0f, 0f) };
    internal static ShaderPropertyValue FromFloat(float v) => new() { Type = ShaderPropertyType.Float, Value = new(v, 0f, 0f, 0f) };
    internal static ShaderPropertyValue FromVector2(Vector2 v) => new() { Type = ShaderPropertyType.Vec2, Value = new(v, 0f, 0f) };
    internal static ShaderPropertyValue FromVector3(Vector3 v) => new() { Type = ShaderPropertyType.Vec3, Value = new(v, 0f) };
    internal static ShaderPropertyValue FromVector4(Vector4 v) => new() { Type = ShaderPropertyType.Vec4, Value = v };
    internal static ShaderPropertyValue FromMatrix4x4(Matrix4x4 m) => new() { Type = ShaderPropertyType.Mat4, Matrix = m };

    internal int AsInt() => (int)Value.X;
    internal float AsFloat() => Value.X;
    internal Vector2 AsVector2() => new(Value.X, Value.Y);
    internal Vector3 AsVector3() => new(Value.X, Value.Y, Value.Z);
    internal Vector4 AsVector4() => Value;
    internal Matrix4x4 AsMatrix4x4() => Matrix;
}