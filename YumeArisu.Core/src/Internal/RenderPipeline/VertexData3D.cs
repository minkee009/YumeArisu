using System.Numerics;
using System.Runtime.InteropServices;

namespace YumeArisu.Core.Internal.RenderPipeline;

[StructLayout(LayoutKind.Sequential)]
internal struct VertexData3D
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 UV;
}

