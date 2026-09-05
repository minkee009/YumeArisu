using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct MaterialMeta
{
    public string ShaderPath { get; init; }
    public Dictionary<string, string> TexturePaths { get; init; }
    public Dictionary<string, UniformValue> Uniforms { get; init; }
    public BlendMode? BlendMode { get; init; }
    public int? RenderQueue { get; init; }
}