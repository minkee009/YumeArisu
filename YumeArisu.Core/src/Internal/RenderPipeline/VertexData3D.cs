using System.Numerics;
using System.Runtime.InteropServices;

namespace YumeArisu.Core.Internal.RenderPipeline;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct VertexData3D
{
    public Vector3 Position { get; init; }
    public Vector3 Normal { get; init; }
    public Vector2 UV { get; init; }

    public VertexData3D(Vector3 position, Vector3 normal, Vector2 uv)
    {
        Position = position;
        Normal = normal;
        UV = uv;
    }
}

