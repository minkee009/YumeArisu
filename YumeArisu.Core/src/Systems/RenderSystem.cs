using System.Drawing;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class RenderSystem : SystemBase<RenderSystem, IView>
{
    private GL _gl;

    // =============================================================
    //  테스트 코드
    //
    
    private uint _pixelVAO = 0;
    private uint _pixelVBO = 0;
    private const string _vertexShader = """
        #version 330 core

        layout(location = 0) in vec2 position;

        out vec2 vPos; // fragment로 넘길 값

        void main()
        {
            vPos = position;
            gl_Position = vec4(position, 0.0, 1.0);
        }
        """;
    private const string _fragmentShader = """
        #version 330 core

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
    
    private Vector2D<int> _frameBufferSize;

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

        _frameBufferSize = view.FramebufferSize;

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

        uint vertexShader = CompileShader(GLEnum.VertexShader, _vertexShader);
        uint fragmentShader = CompileShader(GLEnum.FragmentShader, _fragmentShader);

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
        _gl = null;

        // 카메라 리스트 해제
        // 렌더러 리스트 해제
    }

    public void OnFramebufferResize(Vector2D<int> size)
    {
        _frameBufferSize = size;
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
        _gl.Uniform2(resLoc, _frameBufferSize.X, _frameBufferSize.Y);

        // var windowSize = WindowControl.Size;
        // float sx = (float)_frameBufferSize.X / windowSize.X;
        // float sy = (float)_frameBufferSize.Y / windowSize.Y;

        var mouse = Input.GetMousePosition();
        // mouse = new(mouse.X * sx, mouse.Y * sy);
        // mouse.Y = _frameBufferSize.Y - mouse.Y;
        int mouseLoc = _gl.GetUniformLocation(_program, "iMouse");
        _gl.Uniform2(mouseLoc, mouse.X, _frameBufferSize.Y - mouse.Y);

        int timeLoc = _gl.GetUniformLocation(_program, "iTime");
        _gl.Uniform1(timeLoc, (float)Time.TotalTime);

        // var halfViewSize = (_frameBufferSize / 2);
        // var quaterViewSize = halfViewSize / 2;
        // _gl.Viewport(quaterViewSize, halfViewSize);

        // _gl.Enable(EnableCap.ScissorTest);
        // _gl.Scissor(quaterViewSize.X, quaterViewSize.Y, (uint)halfViewSize.X, (uint)halfViewSize.Y); // 여기에 뷰포트와 같은 값을 넣어야 함

        // _gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
        // _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        // _gl.Disable(EnableCap.ScissorTest); // 이후 그리기에 영향 없도록 꺼줌

        _gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        _gl.Viewport(_frameBufferSize);

        // Camera Clear
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
        _gl.Viewport(_frameBufferSize);
    }

    public GL GetGL() => _gl;


    uint CompileShader(GLEnum type, in string source)
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