using System.Numerics;

namespace YumeArisu.Core.Common;

public readonly struct BoundingBox
{
    public Vector3 Min { get; }
    public Vector3 Max { get; }

    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = Vector3.Min(min, max);
        Max = Vector3.Max(min, max);
    }

    internal IEnumerable<Vector3> GetCorners()
    {
        yield return new Vector3(Min.X, Min.Y, Min.Z);
        yield return new Vector3(Max.X, Min.Y, Min.Z);
        yield return new Vector3(Min.X, Max.Y, Min.Z);
        yield return new Vector3(Max.X, Max.Y, Min.Z);
        yield return new Vector3(Min.X, Min.Y, Max.Z);
        yield return new Vector3(Max.X, Min.Y, Max.Z);
        yield return new Vector3(Min.X, Max.Y, Max.Z);
        yield return new Vector3(Max.X, Max.Y, Max.Z);
    }
}