using BuiltInShader = YumeArisu.Core.Rendering.Shader;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal static class BuiltInRenderResource
{
    #region  ShaderSource
    public const string Vertex = """
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
    public const string Fragment = """
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
    #endregion

    public static BuiltInShader FullScreenQuadShader;

    private static bool _isLoaded;

    public static void Load()
    {
        if (_isLoaded)
            return;
        
        FullScreenQuadShader = new();

        FullScreenQuadShader.ImmediateLoadFromSource(Vertex, Fragment);

        _isLoaded = true;
    }

    public static void Unload()
    {
        if(!_isLoaded)
            return;

        FullScreenQuadShader.Unload();

        FullScreenQuadShader = null;

        _isLoaded = false;
    }
}