using System.Numerics;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct ShaderPropertyMeta
{
    public ShaderPropertyType Type { get; init; }
    public float[] Data { get; init; }

    internal ShaderPropertyValue ToValue()
    {
        if (Data is null)
            throw new InvalidDataException("ShaderProperty 데이터가 없습니다.");

        return Type switch
        {
            ShaderPropertyType.Int when Data.Length == 1 => ShaderPropertyValue.FromInt((int)Data[0]),
            ShaderPropertyType.Float when Data.Length == 1 => ShaderPropertyValue.FromFloat(Data[0]),
            ShaderPropertyType.Vec2 when Data.Length == 2 => ShaderPropertyValue.FromVector2(new(Data[0], Data[1])),
            ShaderPropertyType.Vec3 when Data.Length == 3 => ShaderPropertyValue.FromVector3(new(Data[0], Data[1], Data[2])),
            ShaderPropertyType.Vec4 when Data.Length == 4 => ShaderPropertyValue.FromVector4(new(Data[0], Data[1], Data[2], Data[3])),
            ShaderPropertyType.Mat4 when Data.Length == 16 => ShaderPropertyValue.FromMatrix4x4(new Matrix4x4(
                Data[0], Data[1], Data[2], Data[3],
                Data[4], Data[5], Data[6], Data[7],
                Data[8], Data[9], Data[10], Data[11],
                Data[12], Data[13], Data[14], Data[15])),
            _ => throw new InvalidDataException($"ShaderProperty 데이터 길이가 올바르지 않습니다: {Type}")
        };
    }
}