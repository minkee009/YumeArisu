using StbImageSharp;
using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Rendering;

public class Image : Resource
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public byte[] PixelData { get; private set; }

    protected override bool OnLoad(byte[] bytes)
    {
        // OpenGL 이미지 처리 -> 상-하 뒤집기
        StbImage.stbi_set_flip_vertically_on_load(1);

        ImageResult image = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

        Width = image.Width;
        Height = image.Height;
        PixelData = image.Data; // RGBA 픽셀 바이트 배열

        return true;
    }

    protected override void OnUnload()
    {
        Width = 0;
        Height = 0;
        PixelData = null;
    }
}