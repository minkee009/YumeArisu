namespace YumeArisu.Core.Internal.RenderPipeline;

internal readonly struct MaterialMeta
{
    public string ShaderPath { get; init; }
    public Dictionary<string, string> TexturePaths { get; init; }
    public Dictionary<string, UniformMeta> Uniforms { get; init; }
}