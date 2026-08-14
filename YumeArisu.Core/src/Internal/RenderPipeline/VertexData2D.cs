using System.Numerics;
using System.Runtime.InteropServices;

namespace YumeArisu.Core.Internal.RenderPipeline;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct VertexData2D
{
    public readonly Vector3 Position;
    public readonly Vector2 UV;

    public VertexData2D(Vector3 position, Vector2 uv)
    {
        Position = position;
        UV = uv;
    }
}

