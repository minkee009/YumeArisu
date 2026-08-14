using System.Text;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Internal.ResourceHandling;
using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Rendering;

public class Shader : Resource
{
    internal uint Handle { get; private set; }

    internal bool ImmediateLoadFromSource(string vertBody, string fragBody)
    {
        if(IsLoaded)
            return false;

        IsLoaded = true;
        return true;
    }

    protected override bool OnLoad(byte[] bytes)
    {
        // .shader 파일인지 확인
        if (!PathHelper.HasExtension(Path, ".shader"))
            return false;

        // shader meta로 전환
        var json = Encoding.UTF8.GetString(bytes);
        var meta = JsonMetaParser.Parse<ShaderMeta>(json);

        // 각 부분을 추가로 로딩
        var vertBody = FileIO.ReadAllString(meta.VertBodyPath);
        var fragBody = FileIO.ReadAllString(meta.FragBodyPath);

        if(string.IsNullOrEmpty(vertBody) || string.IsNullOrEmpty(fragBody))
            return false;

        return true;
    }

    protected override void OnUnload()
    {
        throw new NotImplementedException();
    }
}