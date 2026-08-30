using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace YumeArisu.Core.Utility;

public static class SilkMathsToNumerics
{
    public static Vector2 ToNumerics(this Vector2D<int> v) => new(v.X, v.Y); 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToNumerics(this in Vector2D<float> v)
        => Unsafe.As<Vector2D<float>, Vector2>(ref Unsafe.AsRef(in v));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToNumerics(this in Vector3D<float> v)
        => Unsafe.As<Vector3D<float>, Vector3>(ref Unsafe.AsRef(in v));
}