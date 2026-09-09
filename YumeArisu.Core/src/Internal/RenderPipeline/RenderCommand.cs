using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct RenderCommand
{
    internal Mesh Mesh { get; init; }
    internal Material Material { get; init; }
    internal ShaderPropertyBlock MaterialOverride { get; init; }
    internal ShaderPropertyBlock ObjectProperties { get; init; }
    internal float Depth { get; init; }
}