using System.Numerics;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal struct ShaderPropertyValue
{
    public ShaderPropertyType Type { get; init; }
    public float[] Data { get; init; }

    internal static ShaderPropertyValue FromInt(int v) => new() { Type = ShaderPropertyType.Int, Data = new[] { (float)v } };
    internal static ShaderPropertyValue FromFloat(float v) => new() { Type = ShaderPropertyType.Float, Data = new[] { v } };
    internal static ShaderPropertyValue FromVector2(Vector2 v) => new() { Type = ShaderPropertyType.Vec2, Data = new[] { v.X, v.Y } };
    internal static ShaderPropertyValue FromVector3(Vector3 v) => new() { Type = ShaderPropertyType.Vec3, Data = new[] { v.X, v.Y, v.Z } };
    internal static ShaderPropertyValue FromVector4(Vector4 v) => new() { Type = ShaderPropertyType.Vec4, Data = new[] { v.X, v.Y, v.Z, v.W } };
    internal static ShaderPropertyValue FromMatrix4x4(Matrix4x4 m) => new()
    {
        Type = ShaderPropertyType.Mat4,
        Data = new[]
        {
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        }
    };

    internal int AsInt() => (int)Data[0];
    internal float AsFloat() => Data[0];
    internal Vector2 AsVector2() => new(Data[0], Data[1]);
    internal Vector3 AsVector3() => new(Data[0], Data[1], Data[2]);
    internal Vector4 AsVector4() => new(Data[0], Data[1], Data[2], Data[3]);
    internal Matrix4x4 AsMatrix4x4() => new(
        Data[0], Data[1], Data[2], Data[3],
        Data[4], Data[5], Data[6], Data[7],
        Data[8], Data[9], Data[10], Data[11],
        Data[12], Data[13], Data[14], Data[15]);
}