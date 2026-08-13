using System.Drawing;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Systems;

public class RenderSystem : SystemBase<RenderSystem, IView>
{
    public Vector2D<int> FramebufferSize { get; private set; }
    public float FramebufferAspect => FramebufferSize.X / FramebufferSize.Y;
    
    // GL Context
    private GL _gl;
    private ShaderBackend _shaderBackend;

    // Render Pipeline Object
    private List<Camera> _cameras;
    private bool _needCamDepthSort;

    // =============================================================
    //  테스트 코드
    //
    
    private uint _pixelVAO = 0;
    private uint _pixelVBO = 0;
    private const string _vertexShader = """
        #ifdef GLES
        precision mediump float;
        #endif
        layout(location = 0) in vec2 position;

        out vec2 vPos; // fragment로 넘길 값

        void main()
        {
            vPos = position;
            gl_Position = vec4(position, 0.0, 1.0);
        }
        """;
    private const string _fragmentShader = """
        #ifdef GLES
        precision mediump float;
        #endif

        in vec2 vPos;
        out vec4 FragColor;

        uniform vec2 iResolution;
        uniform vec2 iMouse;
        uniform float iTime;

        void main()
        {
            vec2 uv = vPos * 0.5 + 0.5;

            // 기본 ShaderToy 느낌
            vec3 col = vec3(uv, 0.5 + 0.5 * sin(iTime));

            // 마우스 기반 효과
            // float dist = distance(uv, iMouse / iResolution);
            // col += vec3(1.0 - smoothstep(0.0, 0.2, dist));

            FragColor = vec4(col, 1.0);
        }
        """;

    private uint _program = 0;
    private float[] _vertices;

    // =============================================================

    internal override void OnStartUp(IView view)
    {
        try
        {
            _gl = view.CreateOpenGL();
        }
        catch
        {
            Environment.FailFast("GL 컨텍스트를 생성하지 못했습니다.");
            return; // Exit가 비동기 콜백 안에서 즉시 안 먹힐 상황 대비한 안전장치
        }

        _cameras = new List<Camera>();

        string version = _gl.GetStringS(GLEnum.Version);
        _shaderBackend = version.Contains("OpenGL ES") ? ShaderBackend.OpenGLES : ShaderBackend.OpenGLCore;

        _pixelVAO = _gl.GenVertexArray();
        _pixelVBO = _gl.GenBuffer();

        _gl.BindVertexArray(_pixelVAO);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _pixelVBO);

        unsafe
        {
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer, 
                sizeof(float) * 12, 
                null, 
                BufferUsageARB.DynamicDraw
            );

            _gl.VertexAttribPointer(
                0,
                2,
                GLEnum.Float,
                false,
                sizeof(float) * 2, 
                null
            );
        }
        _gl.EnableVertexAttribArray(0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        _gl.BindVertexArray(0);

        uint vertexShader = CompileShader(GLEnum.VertexShader, BuildShader(_vertexShader));
        uint fragmentShader = CompileShader(GLEnum.FragmentShader, BuildShader(_fragmentShader));

        _program = _gl.CreateProgram();

        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);

        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_program)}");
        }
        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    internal override void OnShutDown()
    {
        _cameras.Clear();
        _cameras = null;
        _gl = null;

        // 카메라 리스트 해제
        // 렌더러 리스트 해제
    }

    public void OnFramebufferResize(Vector2D<int> size)
    {
        FramebufferSize = size;
        foreach (var cam in _cameras)
            cam.MarkProjectionMatrixDirty();
    }

    public void BeginFrame()
    {
        // TestCode

        _vertices =
        [
            -1f, -1f,
            1f, -1f,
            1f,  1f,

            -1f, -1f,
            1f,  1f,
            -1f,  1f
        ];

        int resLoc = _gl.GetUniformLocation(_program, "iResolution");
        _gl.Uniform2(resLoc, FramebufferSize.X, FramebufferSize.Y);

        // var windowSize = WindowControl.Size;
        // float sx = (float)_framebufferSize.X / windowSize.X;
        // float sy = (float)_framebufferSize.Y / windowSize.Y;

        var mouse = Input.GetMousePosition();
        // mouse = new(mouse.X * sx, mouse.Y * sy);
        // mouse.Y = _framebufferSize.Y - mouse.Y;
        int mouseLoc = _gl.GetUniformLocation(_program, "iMouse");
        _gl.Uniform2(mouseLoc, mouse.X, FramebufferSize.Y - mouse.Y);

        int timeLoc = _gl.GetUniformLocation(_program, "iTime");
        _gl.Uniform1(timeLoc, (float)Time.TotalTime);

        // var halfViewSize = (_framebufferSize / 2);
        // var quaterViewSize = halfViewSize / 2;
        // _gl.Viewport(quaterViewSize, halfViewSize);

        // _gl.Enable(EnableCap.ScissorTest);
        // _gl.Scissor(quaterViewSize.X, quaterViewSize.Y, (uint)halfViewSize.X, (uint)halfViewSize.Y); // 여기에 뷰포트와 같은 값을 넣어야 함

        // _gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
        // _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        // _gl.Disable(EnableCap.ScissorTest); // 이후 그리기에 영향 없도록 꺼줌

        _gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        _gl.Viewport(FramebufferSize);

        // 렌더 오브젝트 정렬
        if (_needCamDepthSort)
        {
            _cameras.Sort((a,b) => a.Depth.CompareTo(b.Depth));
            _needCamDepthSort = false;
        }
    }

    public void Render()
    {
        _gl.UseProgram(_program);

        _gl.BindVertexArray(_pixelVAO);

        _gl.BindBuffer(GLEnum.ArrayBuffer, _pixelVBO);

        unsafe
        {
            fixed (float* ptr = _vertices)
            {
                
                _gl.BufferSubData(
                    GLEnum.ArrayBuffer,
                    0,
                    (nuint)(_vertices.Length * sizeof(float)),
                    ptr
                );
            }
        }

        _gl.DrawArrays(
            GLEnum.Triangles,
            0,
            6
        );
    }

    public void EndFrame()
    {
        _gl.Viewport(FramebufferSize);
    }

    public GL GetGL() => _gl;

    internal void RegisterCamera(Camera camera)
    {
        _cameras.Add(camera);
        _needCamDepthSort = true;
    } 

    internal void UnregisterCamera(Camera camera) => _cameras.Remove(camera);

    internal ShaderBackend GetShaderBackend() => _shaderBackend;

    private string BuildShader(string src)
    {
        string version = (_shaderBackend == ShaderBackend.OpenGLES)
            ? "#version 300 es\n"
            : "#version 330 core\n";

        string define = (_shaderBackend == ShaderBackend.OpenGLES)
            ? "#define GLES\n"
            : "#define GLCORE\n";

        return version + define + src;
    }

    private uint CompileShader(GLEnum type, in string source)
    {
        uint shader = _gl.CreateShader(type);

        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        string infoLog = _gl.GetShaderInfoLog(shader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return shader;
    }
}

// 문법 설탕용 클래스
public static class Screen
{
    public static Vector2D<int> Resolution => RenderSystem.Instance.FramebufferSize;
    public static float Aspect => RenderSystem.Instance.FramebufferAspect;
}