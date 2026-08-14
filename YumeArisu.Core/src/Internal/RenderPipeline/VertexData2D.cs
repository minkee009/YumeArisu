using System.Numerics;
using System.Runtime.InteropServices;

namespace YumeArisu.Core.Internal.RenderPipeline;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct VertexData2D
{
    public Vector3 Position { get; init; }
    public Vector2 UV { get; init; }
}

