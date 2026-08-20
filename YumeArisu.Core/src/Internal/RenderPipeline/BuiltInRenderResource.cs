using YumeArisu.Core.Rendering;
using BuiltInShader = YumeArisu.Core.Rendering.Shader;
using BuiltInTexture = YumeArisu.Core.Rendering.Texture;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal static class BuiltInRenderResource
{
    #region  ShaderSource
    public const string TestVertex = """
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
    public const string TestFragment = """
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

    public const string SpriteVertex = $$"""
        layout(location = 0) in vec3 position;
        layout(location = 1) in vec2 uv;

        out vec2 fragUV;

        void main()
        {
            fragUV = uv;
            gl_Position = {{GlobalUniform.Projection}} * {{GlobalUniform.View}} * {{GlobalUniform.Model}} * vec4(position, 1.0);
        }
        """;

    public const string SpriteFragment = """
        in vec2 fragUV;

        uniform sampler2D MainTexture;

        out vec4 FragColor;

        void main()
        {
            FragColor = texture(MainTexture, fragUV);
        }
        """;
    #endregion

    public static BuiltInShader DefaultSpriteShader;
    public static BuiltInShader FullScreenQuadShader;
    public static BuiltInTexture DefaultWhiteTexture;
    public static Mesh DefaultQuadMesh;
    public static Material DefaultSpriteMaterial;

    private static bool _isLoaded;

    public static void Load()
    {
        if (_isLoaded)
            return;
        
        DefaultSpriteShader = new();
        FullScreenQuadShader = new();
        DefaultWhiteTexture = new();
        DefaultQuadMesh = new();
        DefaultSpriteMaterial = new();

        DefaultWhiteTexture.ImmediateLoadToSolidColor(1.0f,1.0f,1.0f,1.0f);

        VertexLayout layout;
        float[] vertices;
        uint[] indices;

        layout = new VertexLayout
        {
            Elements =
            [
                new VertexElement
                {
                    Location = 0,
                    Name = "Position",
                    Type = VertexElementType.Float2
                }
            ]
        };


        FullScreenQuadShader.ImmediateLoadFromSource(
            layout,
            TestVertex,
            TestFragment);


        layout = new VertexLayout
        {
            Elements =
            [
                new VertexElement
                {
                    Location = 0,
                    Name = "Position",
                    Type = VertexElementType.Float3
                },
                new VertexElement
                {
                    Location = 1,
                    Name = "UV",
                    Type = VertexElementType.Float2
                }
            ]
        };

        vertices = 
        [
            // Position          // UV
            -0.5f, -0.5f, 0f,    0f, 0f,
            0.5f, -0.5f, 0f,    1f, 0f,
            0.5f,  0.5f, 0f,    1f, 1f,
            -0.5f,  0.5f, 0f,    0f, 1f
        ];

        indices = 
        [
            0, 1, 2,
            2, 3, 0
        ];

        DefaultQuadMesh.ImmediatLoadFromReference(
            layout,
            vertices,
            indices);

        
        DefaultSpriteShader.ImmediateLoadFromSource(layout,SpriteVertex,SpriteFragment);

        DefaultSpriteMaterial.ImmediateLoadFromReference(DefaultSpriteShader, 
            new()
            {
                ["MainTexture"] = DefaultWhiteTexture
            },
            new());
    
        _isLoaded = true;
    }

    public static void Unload()
    {
        if (!_isLoaded)
            return;

        FullScreenQuadShader.Unload();
        DefaultWhiteTexture.Unload();
        DefaultQuadMesh.Unload();
        DefaultSpriteMaterial.Unload();
        DefaultSpriteShader.Unload();

        FullScreenQuadShader = null;
        DefaultWhiteTexture = null;

        _isLoaded = false;
    }
}