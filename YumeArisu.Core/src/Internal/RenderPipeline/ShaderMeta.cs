namespace YumeArisu.Core.Internal.RenderPipeline;

internal struct ShaderMeta
{
    public string VertBodyPath;
    public string FragBodyPath;

    public ShaderMeta(string vertBodyPath, string fragBodyPath)
    {
        VertBodyPath = vertBodyPath;
        FragBodyPath = fragBodyPath;
    }
}