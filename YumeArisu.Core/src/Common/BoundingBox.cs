using System.Numerics;
using System.Text.Json.Serialization;

namespace YumeArisu.Core.Common;

/// <summary>
/// 축 정렬 바운딩 박스(AABB)입니다. 공간(로컬/월드)은 사용하는 쪽에서 구분합니다.
/// default 값은 원점의 점(Min = Max = 0)이므로 누적용 초기값으로 쓰지 말고, 첫 점으로 시작하세요.
/// </summary>
public readonly struct BoundingBox : IEquatable<BoundingBox>
{
    public Vector3 Min { get; }
    public Vector3 Max { get; }

    public Vector3 Center => (Min + Max) * 0.5f;
    public Vector3 Extents => (Max - Min) * 0.5f;
    public Vector3 Size => Max - Min;

    [JsonConstructor]
    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = Vector3.Min(min, max);
        Max = Vector3.Max(min, max);
    }

    public static BoundingBox FromCenterExtents(Vector3 center, Vector3 extents)
    {
        extents = Vector3.Abs(extents);
        return new BoundingBox(center - extents, center + extents);
    }

    public bool Contains(Vector3 point)
        => point.X >= Min.X && point.X <= Max.X
        && point.Y >= Min.Y && point.Y <= Max.Y
        && point.Z >= Min.Z && point.Z <= Max.Z;

    public bool Contains(in BoundingBox other)
        => other.Min.X >= Min.X && other.Max.X <= Max.X
        && other.Min.Y >= Min.Y && other.Max.Y <= Max.Y
        && other.Min.Z >= Min.Z && other.Max.Z <= Max.Z;

    public bool Intersects(in BoundingBox other)
        => Min.X <= other.Max.X && Max.X >= other.Min.X
        && Min.Y <= other.Max.Y && Max.Y >= other.Min.Y
        && Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    public BoundingBox Encapsulate(Vector3 point) => new(Vector3.Min(Min, point), Vector3.Max(Max, point));

    public BoundingBox Encapsulate(in BoundingBox other) => new(Vector3.Min(Min, other.Min), Vector3.Max(Max, other.Max));

    /// <summary>
    /// 행렬(System.Numerics row-vector 규약)로 변환한 뒤의 AABB를 반환합니다.
    /// 8코너를 돌지 않고 center/extents 방식으로 계산하므로 할당이 없습니다.
    /// 회전이 있으면 결과 박스는 원본보다 커질 수 있습니다(보수적).
    /// </summary>
    public BoundingBox Transform(in Matrix4x4 m)
    {
        var c = Center;
        var e = Extents;

        var newCenter = Vector3.Transform(c, m);
        var newExtents = new Vector3(
            e.X * MathF.Abs(m.M11) + e.Y * MathF.Abs(m.M21) + e.Z * MathF.Abs(m.M31),
            e.X * MathF.Abs(m.M12) + e.Y * MathF.Abs(m.M22) + e.Z * MathF.Abs(m.M32),
            e.X * MathF.Abs(m.M13) + e.Y * MathF.Abs(m.M23) + e.Z * MathF.Abs(m.M33));

        return FromCenterExtents(newCenter, newExtents);
    }

    /// <summary>
    /// 8개 코너를 주어진 Span에 채웁니다. (길이 8 이상 필요, 할당 없음)
    /// </summary>
    public void GetCorners(Span<Vector3> corners)
    {
        if (corners.Length < 8)
            throw new ArgumentException("코너를 담으려면 길이 8 이상의 Span이 필요합니다.", nameof(corners));

        corners[0] = new Vector3(Min.X, Min.Y, Min.Z);
        corners[1] = new Vector3(Max.X, Min.Y, Min.Z);
        corners[2] = new Vector3(Min.X, Max.Y, Min.Z);
        corners[3] = new Vector3(Max.X, Max.Y, Min.Z);
        corners[4] = new Vector3(Min.X, Min.Y, Max.Z);
        corners[5] = new Vector3(Max.X, Min.Y, Max.Z);
        corners[6] = new Vector3(Min.X, Max.Y, Max.Z);
        corners[7] = new Vector3(Max.X, Max.Y, Max.Z);
    }

    public bool Equals(BoundingBox other) => Min == other.Min && Max == other.Max;
    public override bool Equals(object obj) => obj is BoundingBox other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Min, Max);
    public override string ToString() => $"Min : {Min}, Max : {Max}";

    public static bool operator ==(BoundingBox left, BoundingBox right) => left.Equals(right);
    public static bool operator !=(BoundingBox left, BoundingBox right) => !left.Equals(right);
}