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

        UploadToGL(1, 1, [r, g, b, a]);

        IsLoaded = true;
        return true;
    }

    internal bool ImmediateLoadToSolidColor(System.Drawing.Color color)
    {
        return ImmediateLoadToSolidColor(color.R, color.G, color.B, color.A);
    }

    internal bool ImmediateLoadToSolidColor(int r, int g, int b, int a = 255)
    {
        return ImmediateLoadToSolidColor(
            (byte)Math.Clamp(r, 0, 255),
            (byte)Math.Clamp(g, 0, 255),
            (byte)Math.Clamp(b, 0, 255),
            (byte)Math.Clamp(a, 0, 255)
        );
    }

    internal bool ImmediateLoadToSolidColor(float r, float g, float b, float a = 1.0f)
    {
        // float -> byte 변환 (반올림 및 Clamp 처리)
        byte byteR = (byte)(Math.Clamp(r, 0.0f, 1.0f) * 255.0f + 0.5f);
        byte byteG = (byte)(Math.Clamp(g, 0.0f, 1.0f) * 255.0f + 0.5f);
        byte byteB = (byte)(Math.Clamp(b, 0.0f, 1.0f) * 255.0f + 0.5f);
        byte byteA = (byte)(Math.Clamp(a, 0.0f, 1.0f) * 255.0f + 0.5f);

        return ImmediateLoadToSolidColor(byteR, byteG, byteB, byteA);
    }

    /// <summary>
    /// 원시 바이트 배열을 GL 컨텍스트에 제출하여 텍스쳐로 생성합니다.
    /// </summary>
    internal bool ImmediateLoadFromBytes(byte[] rawBytes, int width, int height)
    {
        if (IsLoaded)
            return false;

        UploadToGL(width, height, rawBytes);

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