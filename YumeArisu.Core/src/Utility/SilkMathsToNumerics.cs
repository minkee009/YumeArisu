using System.Numerics;
using Silk.NET.Maths;

namespace YumeArisu.Core.Utility;

public static class SilkMathsToNumerics
{
    public static Vector2 ToNumerics(this Vector2D<float> v) => new(v.X, v.Y);

    public static Vector2 ToNumerics(this Vector2D<int> v) => new(v.X, v.Y); 

    public static Vector3 ToNumerics(this Vector3D<float> v) => new(v.X, v.Y, v.Z);
}