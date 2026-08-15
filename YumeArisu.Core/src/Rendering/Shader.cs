using System.Text;
using Silk.NET.OpenGL;
using YumeArisu.Core.Systems;
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

        Handle = LinkShaderProgram(vertBody, fragBody);

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

        Handle = LinkShaderProgram(vertBody, fragBody);

        return true;
    }

    protected override void OnUnload()
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.DeleteProgram(Handle);
        Handle = 0;
    }

    internal void Use()
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.UseProgram(Handle);
    }

    internal static uint LinkShaderProgram(string vertBody, string fragBody)
    {
        var vertexShader = CompileShader(GLEnum.VertexShader, BuildShader(vertBody));
        var fragmentShader = CompileShader(GLEnum.FragmentShader, BuildShader(fragBody));

        var gl = RenderSystem.Instance.GetGL();

        uint handle = gl.CreateProgram();

        gl.AttachShader(handle, vertexShader);
        gl.AttachShader(handle, fragmentShader);
        
        gl.LinkProgram(handle);
        gl.GetProgram(handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            string programLog = gl.GetProgramInfoLog(handle);
            string vertInfoLog = gl.GetShaderInfoLog(vertexShader);
            string fragInfoLog = gl.GetShaderInfoLog(fragmentShader);
            gl.DeleteProgram(handle); // 실패했으면 핸들도 정리
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
            throw new Exception($"셰이더 핸들을 가져오는데 실패했습니다 - 셰이더 프로그램 링크가 성공하지 않음: {programLog} :: vert log - {vertInfoLog} / frag log - {fragInfoLog}");
        }
        gl.DetachShader(handle, vertexShader);
        gl.DetachShader(handle, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        return handle;
    }

    internal static string BuildShader(string src)
    {
        var shaderBackend = RenderSystem.Instance.GetShaderBackend();

        string version = (shaderBackend == ShaderBackend.OpenGLES)
            ? "#version 300 es\n"
            : "#version 330 core\n";

        string define = (shaderBackend == ShaderBackend.OpenGLES)
            ? "#define GLES\n"
            : "#define GLCORE\n";

        return version + define + src;
    }

    internal static uint CompileShader(GLEnum type, in string source)
    {
        var gl = RenderSystem.Instance.GetGL();
        uint shader = gl.CreateShader(type);

        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        gl.GetShader(shader, GLEnum.CompileStatus, out int status);
        if (status == 0)
        {
            string infoLog = gl.GetShaderInfoLog(shader);
            gl.DeleteShader(shader); // 실패한 shader object도 정리
            throw new Exception($"셰이더 타입 {type} 컴파일 실패: {infoLog}");
        }

        return shader;
    }
}