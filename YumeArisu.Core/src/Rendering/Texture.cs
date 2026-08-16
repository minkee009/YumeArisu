using Silk.NET.OpenGL;
using StbImageSharp;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Rendering;

public class Texture : Resource
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public TextureRenderMode TextureRenderMode 
    { 
        get => _textureRenderMode;
        set
        {
            if (_textureRenderMode == value)
                return;

            _textureRenderMode = value;

            if (IsLoaded)
                ApplyFilterMode();
        }
    }

    internal uint Handle { get; private set; }

    private TextureRenderMode _textureRenderMode = TextureRenderMode.Bilinear;

    internal void Bind(uint slot = 0)
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.ActiveTexture(TextureUnit.Texture0 + (int)slot);
        gl.BindTexture(GLEnum.Texture2D, Handle);
    }

    /// <summary>
    /// 단색 1x1 텍스쳐를 즉시 생성합니다. (기본/폴백 텍스쳐 용도)
    /// </summary>
    internal bool ImmediateLoadToSolidColor(byte r, byte g, byte b, byte a)
    {
        if (IsLoaded)
            return false;

        UploadToGL(1, 1, new byte[] { r, g, b, a });

        IsLoaded = true;
        return true;
    }

    /// <summary>
    /// 인코딩된 이미지 바이트(png, jpg 등)를 즉시 디코딩하여 텍스쳐로 생성합니다.
    /// </summary>
    internal bool ImmediateLoadFromBytes(byte[] bytes)
    {
        if (IsLoaded)
            return false;

        StbImage.stbi_set_flip_vertically_on_load(1);
        var image = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

        UploadToGL(image.Width, image.Height, image.Data);

        IsLoaded = true;
        return true;
    }

    protected override bool OnLoad(byte[] bytes)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        var image = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

        UploadToGL(image.Width, image.Height, image.Data);

        return true;
    }

    protected override void OnUnload()
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.DeleteTexture(Handle);
        Handle = 0;
        Width = 0;
        Height = 0;
    }

    private unsafe void UploadToGL(int width, int height, byte[] pixelData)
    {
        Width = width;
        Height = height;

        var gl = RenderSystem.Instance.GetGL();

        Handle = gl.GenTexture();
        gl.BindTexture(GLEnum.Texture2D, Handle);

        fixed (byte* ptr = pixelData)
        {
            gl.TexImage2D(
                GLEnum.Texture2D, 0, InternalFormat.Rgba8,
                (uint)Width, (uint)Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }

        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);

        gl.GenerateMipmap(GLEnum.Texture2D);

        ApplyFilterMode();

        gl.BindTexture(GLEnum.Texture2D, 0);
    }

    private void ApplyFilterMode()
    {
        var gl = RenderSystem.Instance.GetGL();

        int minFilter = _textureRenderMode switch
        {
            TextureRenderMode.Point => (int)GLEnum.Nearest,
            TextureRenderMode.Bilinear => (int)GLEnum.Linear,
            TextureRenderMode.Trilinear => (int)GLEnum.LinearMipmapLinear,
            _ => (int)GLEnum.Nearest
        };

        // Mag는 밉맵 개념이 없어서 Trilinear든 Bilinear든 Linear로 동일
        int magFilter = _textureRenderMode == TextureRenderMode.Point
            ? (int)GLEnum.Nearest
            : (int)GLEnum.Linear;

        gl.BindTexture(GLEnum.Texture2D, Handle);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, minFilter);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, magFilter);
    }
}