using System.Text;
using System.Numerics;
using Silk.NET.OpenGL;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Internal.ResourceHandling;
using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Rendering;

public class Shader : Resource
{
    internal uint Handle { get; private set; }

    private readonly Dictionary<string, int> _uniformLocations = new();

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

    internal void SetInt(string name, int value)
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.Uniform1(GetUniformLocation(name), value);
    }

    internal void SetFloat(string name, float value)
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.Uniform1(GetUniformLocation(name), value);
    }

    internal void SetVector2(string name, float x, float y)
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.Uniform2(GetUniformLocation(name), x, y);
    }

    internal void SetVector3(string name, float x, float y, float z)
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.Uniform3(GetUniformLocation(name), x, y, z);
    }

    internal void SetVector4(string name, float x, float y, float z, float w)
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.Uniform4(GetUniformLocation(name), x, y, z, w);
    }

    internal unsafe void SetMatrix4(string name, in Matrix4x4 value)
    {
        var gl = RenderSystem.Instance.GetGL();
        fixed (float* ptr = &value.M11)
        {
            // Numerics는 row-vector 관례, GLSL은 column-major 기대 -> 경계에서만 transpose
            gl.UniformMatrix4(GetUniformLocation(name), 1, true, ptr);
        }
    }

    /// <summary>
    /// UniformValue(Material에서 넘어오는 범용 값)를 타입에 맞춰 실제 glUniform 호출로 분기시킵니다.
    /// </summary>
    internal void SetUniform(string name, UniformValue value)
    {
        switch (value.Type)
        {
            case Internal.RenderPipeline.UniformType.Float:
                SetFloat(name, value.Data[0]);
                break;
            case Internal.RenderPipeline.UniformType.Vec2:
                SetVector2(name, value.Data[0], value.Data[1]);
                break;
            case Internal.RenderPipeline.UniformType.Vec3:
                SetVector3(name, value.Data[0], value.Data[1], value.Data[2]);
                break;
            case Internal.RenderPipeline.UniformType.Vec4:
                SetVector4(name, value.Data[0], value.Data[1], value.Data[2], value.Data[3]);
                break;
            case Internal.RenderPipeline.UniformType.Int:
                SetInt(name, (int)value.Data[0]);
                break;
            case Internal.RenderPipeline.UniformType.Mat4:
                SetMatrix4(name, ToMatrix4x4(value.Data));
                break;
        }
    }

    /// <summary>
    /// 텍스쳐 슬롯(sampler) uniform 설정 - 실제 텍스쳐 바인딩은 Texture.Bind(unit)에서 처리하고,
    /// 여기서는 셰이더의 sampler uniform에 유닛 인덱스만 알려줍니다.
    /// </summary>
    internal void SetTextureUnit(string name, int unit) => SetInt(name, unit);

    internal void SetTexture(string name, Texture texture, int unit)
    {
        texture.Bind((uint)unit);
        SetTextureUnit(name, unit);
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

    private int GetUniformLocation(string name)
    {
        if (_uniformLocations.TryGetValue(name, out int cached))
            return cached;

        var gl = RenderSystem.Instance.GetGL();
        int location = gl.GetUniformLocation(Handle, name);

        // -1이어도 캐싱함 (셰이더에 존재하지 않는 이름 -> 매번 재조회하는 낭비 방지)
        _uniformLocations[name] = location;

        if (location == -1)
            ConsoleExtensions.WriteLineColored($"'{name}' uniform이 셰이더에 존재하지 않습니다.", ConsoleColor.Yellow);

        return location;
    }

    private static Matrix4x4 ToMatrix4x4(float[] d) => new(
        d[0],  d[1],  d[2],  d[3],
        d[4],  d[5],  d[6],  d[7],
        d[8],  d[9],  d[10], d[11],
        d[12], d[13], d[14], d[15]);
}