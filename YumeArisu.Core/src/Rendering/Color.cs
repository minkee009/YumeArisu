using System.Numerics;

namespace YumeArisu.Core.Rendering;

/// <summary>
/// RGBA 색상 (각 채널 0~1 정규화 float)
/// </summary>
public readonly struct Color
{
    public float R { get; init; }
    public float G { get; init; }
    public float B { get; init; }
    public float A { get; init; }

    public Color(float r, float g, float b, float a = 1f)
    {
        R = r; G = g; B = b; A = a;
    }

    public Vector4 ToVector4() => new Vector4(R, G, B, A);

    public static readonly Color White = new() { R = 1, G = 1, B = 1, A = 1 };
    public static readonly Color Black = new() { R = 0, G = 0, B = 0, A = 1 };
    public static readonly Color Red = new() { R = 1, G = 0, B = 0, A = 1 };
    public static readonly Color Green = new() { R = 0, G = 1, B = 0, A = 1 };
    public static readonly Color Blue = new() { R = 0, G = 0, B = 1, A = 1 };
    public static readonly Color Yellow = new() { R = 1, G = 1, B = 0, A = 1 };
    public static readonly Color Cyan = new() { R = 0, G = 1, B = 1, A = 1 };
    public static readonly Color Magenta = new() { R = 1, G = 0, B = 1, A = 1 };

    public static Color FromBytes(byte r, byte g, byte b, byte a = 255) => new(r / 255f, g / 255f, b / 255f, a / 255f);
}