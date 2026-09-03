using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Internal.RenderPipeline;

public static class BuiltInRenderResources
{
    #region  ShaderSource
    public const string SpriteVertex = $$"""
        layout(location = 0) in vec3 position;
        layout(location = 1) in vec2 uv;
        layout(location = 2) in mat4 Model;
        layout(location = 6) in vec2 SpriteSize;
        layout(location = 7) in vec2 SpritePivot;
        layout(location = 8) in vec4 UVRect;
        layout(location = 9) in float FlipX;
        layout(location = 10) in float FlipY;
        layout(location = 11) in vec4 Color;

        out vec2 fragUV;
        out vec4 fragColor;

        void main()
        {
            vec2 local = (position.xy - SpritePivot) * SpriteSize;

            vec2 flippedUV = uv;
            if (FlipX != 0.0) flippedUV.x = 1.0 - flippedUV.x;
            if (FlipY != 0.0) flippedUV.y = 1.0 - flippedUV.y;
            fragUV = UVRect.xy + flippedUV * UVRect.zw;
            fragColor = Color;

            gl_Position = {{GlobalUniform.Projection}} * {{GlobalUniform.View}} * Model * vec4(local, position.z, 1.0);
        }
        """;

    public const string SpriteFragment = """
        uniform sampler2D MainTexture;

        in vec2 fragUV;
        in vec4 fragColor;

        out vec4 FragColor;

        void main()
        {
            FragColor = texture(MainTexture, fragUV) * fragColor;
        }
        """;
    #endregion

    public static Shader DefaultSpriteShader;
    public static Texture DefaultWhiteTexture;
    public static Mesh DefaultQuadMesh;
    public static Material DefaultSpriteMaterial;

    private static bool _isLoaded;

    internal static void Load()
    {
        if (_isLoaded)
            return;
        
        DefaultSpriteShader = new();
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
            0f, 0f, 0f,       0f, 0f,
            1f, 0f, 0f,       1f, 0f,
            1f, 1f, 0f,       1f, 1f,
            0f, 1f, 0f,       0f, 1f
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

        
        DefaultSpriteShader.ImmediateLoadFromReference(layout,SpriteVertex,SpriteFragment);

        DefaultSpriteMaterial.ImmediateLoadFromReference(DefaultSpriteShader, 
            new()
            {
                ["MainTexture"] = DefaultWhiteTexture
            },
            new());
    
        _isLoaded = true;
    }

    internal static void Unload()
    {
        if (!_isLoaded)
            return;

        DefaultWhiteTexture.Unload();
        DefaultQuadMesh.Unload();
        DefaultSpriteMaterial.Unload();
        DefaultSpriteShader.Unload();

        DefaultWhiteTexture = null;
        DefaultQuadMesh = null;
        DefaultSpriteMaterial = null;
        DefaultSpriteShader = null;

        _isLoaded = false;
    }
}