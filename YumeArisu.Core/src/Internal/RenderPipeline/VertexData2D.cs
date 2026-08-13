using System.Numerics;
using System.Runtime.InteropServices;

namespace YumeArisu.Core.Internal.RenderPipeline;

[StructLayout(LayoutKind.Sequential)]
internal struct VertexData2D
{
    public Vector3 Position;
    public Vector2 UV;
}

