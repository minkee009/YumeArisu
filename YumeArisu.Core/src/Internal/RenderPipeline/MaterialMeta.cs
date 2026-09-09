using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct MaterialMeta
{
    public string ShaderPath { get; init; }
    public Dictionary<string, string> TexturePaths { get; init; }
    public Dictionary<string, ShaderPropertyValue> ShaderProperties { get; init; }
    public BlendMode? BlendMode { get; init; }
    public RenderQueue? RenderQueue { get; init; }
}