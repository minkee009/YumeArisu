using System.Numerics;

namespace YumeArisu.Core.Common;

public enum ContainmentType
{
    Outside,
    Intersects,
    Contains
}

/// <summary>
/// 뷰-프로젝션 행렬에서 추출한 6개 평면의 프러스텀입니다.
/// 모든 평면 법선은 안쪽을 향하며, 정규화되어 있습니다.
/// </summary>
public readonly struct Frustum
{
    public Plane Left { get; }
    public Plane Right { get; }
    public Plane Bottom { get; }
    public Plane Top { get; }
    public Plane Near { get; }
    public Plane Far { get; }

    private Frustum(Plane left, Plane right, Plane bottom, Plane top, Plane near, Plane far)
    {
        Left = left;
        Right = right;
        Bottom = bottom;
        Top = top;
        Near = near;
        Far = far;
    }

    /// <summary>
    /// View * Projection 행렬(System.Numerics row-vector 규약)에서 프러스텀을 추출합니다. (Gribb-Hartmann)
    /// </summary>
    /// <param name="vp">View * Projection 행렬</param>
    /// <param name="zeroToOneDepth">
    /// false(기본) : 클립 z가 -w..w (OpenGL, Camera.ProjectionMatrix가 이 범위입니다)
    /// true : 클립 z가 0..w (System.Numerics 원본 / D3D / Vulkan)
    /// </param>
    public static Frustum FromViewProjection(in Matrix4x4 vp, bool zeroToOneDepth = false)
    {
        // clip = v * M 이므로 각 클립 성분은 M의 "열"과의 내적입니다.
        // 안쪽 조건: -w <= x,y,z <= w  ->  (col4 ± colN) 이 평면 계수(a, b, c, d)
        var left = MakePlane(vp.M14 + vp.M11, vp.M24 + vp.M21, vp.M34 + vp.M31, vp.M44 + vp.M41);
        var right = MakePlane(vp.M14 - vp.M11, vp.M24 - vp.M21, vp.M34 - vp.M31, vp.M44 - vp.M41);
        var bottom = MakePlane(vp.M14 + vp.M12, vp.M24 + vp.M22, vp.M34 + vp.M32, vp.M44 + vp.M42);
        var top = MakePlane(vp.M14 - vp.M12, vp.M24 - vp.M22, vp.M34 - vp.M32, vp.M44 - vp.M42);
        var far = MakePlane(vp.M14 - vp.M13, vp.M24 - vp.M23, vp.M34 - vp.M33, vp.M44 - vp.M43);

        var near = zeroToOneDepth
            ? MakePlane(vp.M13, vp.M23, vp.M33, vp.M43)
            : MakePlane(vp.M14 + vp.M13, vp.M24 + vp.M23, vp.M34 + vp.M33, vp.M44 + vp.M43);

        return new Frustum(left, right, bottom, top, near, far);
    }

    public bool Contains(Vector3 point)
        => Plane.DotCoordinate(Left, point) >= 0f
        && Plane.DotCoordinate(Right, point) >= 0f
        && Plane.DotCoordinate(Bottom, point) >= 0f
        && Plane.DotCoordinate(Top, point) >= 0f
        && Plane.DotCoordinate(Near, point) >= 0f
        && Plane.DotCoordinate(Far, point) >= 0f;

    public ContainmentType Contains(Vector3 center, float radius)
    {
        bool partial = false;

        if (IsOutside(Left, center, radius, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Right, center, radius, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Bottom, center, radius, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Top, center, radius, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Near, center, radius, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Far, center, radius, ref partial)) return ContainmentType.Outside;

        return partial ? ContainmentType.Intersects : ContainmentType.Contains;
    }

    public ContainmentType Contains(in BoundingBox box)
    {
        var center = box.Center;
        var extents = box.Extents;
        bool partial = false;

        if (IsOutside(Left, center, extents, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Right, center, extents, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Bottom, center, extents, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Top, center, extents, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Near, center, extents, ref partial)) return ContainmentType.Outside;
        if (IsOutside(Far, center, extents, ref partial)) return ContainmentType.Outside;

        return partial ? ContainmentType.Intersects : ContainmentType.Contains;
    }

    /// <summary>
    /// 박스가 프러스텀과 조금이라도 겹치는지 검사합니다. (컬링용, 보수적: 모서리 근처에서 false positive 가능)
    /// </summary>
    public bool Intersects(in BoundingBox box) => Contains(box) != ContainmentType.Outside;

    public bool Intersects(Vector3 center, float radius) => Contains(center, radius) != ContainmentType.Outside;

    private static Plane MakePlane(float a, float b, float c, float d)
    {
        var normal = new Vector3(a, b, c);
        float length = normal.Length();

        if (length < 1e-6f)
            return new Plane(Vector3.Zero, d);

        float inv = 1f / length;
        return new Plane(normal * inv, d * inv);
    }

    // AABB : 평면 법선 방향으로의 투영 반지름(|n|·e)으로 판정합니다.
    private static bool IsOutside(in Plane plane, in Vector3 center, in Vector3 extents, ref bool partial)
    {
        float distance = Plane.DotCoordinate(plane, center);
        float radius =
            extents.X * MathF.Abs(plane.Normal.X) +
            extents.Y * MathF.Abs(plane.Normal.Y) +
            extents.Z * MathF.Abs(plane.Normal.Z);

        if (distance < -radius)
            return true;

        if (distance < radius)
            partial = true;

        return false;
    }

    private static bool IsOutside(in Plane plane, in Vector3 center, float radius, ref bool partial)
    {
        float distance = Plane.DotCoordinate(plane, center);

        if (distance < -radius)
            return true;

        if (distance < radius)
            partial = true;

        return false;
    }
}