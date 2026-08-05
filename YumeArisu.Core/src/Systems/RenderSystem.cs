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

        void main()
        {
            gl_Position = vec4(position, 0.0, 1.0);
        }
        """;
    private const string _fragmentShader = """
        #version 330 core

        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(1.0, 0.0, 0.8, 1.0);
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
            _gl = GL.GetApi(view);
        }
        catch
        {
            Environment.FailFast("GL 컨텍스트를 찾지 못했습니다. 그래픽스 API는 OpenGL를 사용해야합니다.");
            return; // Exit가 비동기 콜백 안에서 즉시 안 먹힐 상황 대비한 안전장치
        }

        _frameBufferSize = view.FramebufferSize / 2;

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
        _frameBufferSize = size * 2;
        System.Console.WriteLine($"fb - {_frameBufferSize} / window size - {size}");
    }

    public void BeginFrame()
    {

        // TestCode
        var windowSize = WindowControl.Size;
        float sx = (float)_frameBufferSize.X / windowSize.X;
        float sy = (float)_frameBufferSize.Y / windowSize.Y;

        var mousePos = Input.GetMousePosition();
        float px = mousePos.X * sx;
        float py = mousePos.Y * sy; 

        py = _frameBufferSize.Y - py;

        float half = 3f;

        float left   = px - half;
        float right  = px + half;

        float bottom = py - half;
        float top    = py + half;

        var ToNdcX = (float x) => x / _frameBufferSize.X * 2.0f - 1.0f;
        var ToNdcY = (float y) => y / _frameBufferSize.Y * 2.0f - 1.0f;

        _vertices =
        [
            ToNdcX(left),  ToNdcY(bottom),
            ToNdcX(right), ToNdcY(bottom),
            ToNdcX(right), ToNdcY(top),

            ToNdcX(left),  ToNdcY(bottom),
            ToNdcX(right), ToNdcY(top),
            ToNdcX(left),  ToNdcY(top)
        ];
    }

    public void Render()
    {
        var halfViewSize = (_frameBufferSize / 2);
        _gl.Viewport(halfViewSize,_frameBufferSize);

        _gl.Enable(EnableCap.ScissorTest);
        _gl.Scissor(halfViewSize.X, halfViewSize.Y, (uint)_frameBufferSize.X, (uint)_frameBufferSize.Y); // 여기에 뷰포트와 같은 값을 넣어야 함

        _gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        _gl.Disable(EnableCap.ScissorTest); // 이후 그리기에 영향 없도록 꺼줌

        // Camera Clear

        _gl.UseProgram(_program);

        _gl.BindVertexArray(_pixelVAO);

        _gl.BindBuffer(GLEnum.ArrayBuffer, _pixelVBO);

        unsafe
        {
            fixed(float* ptr = _vertices)
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

        //_gl.Viewport(_frameBufferSize / 2);
    }

    public void EndFrame()
    {
        
    }

    public GL GetGL() => _gl;


    unsafe uint CompileShader(GLEnum type, in string source)
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