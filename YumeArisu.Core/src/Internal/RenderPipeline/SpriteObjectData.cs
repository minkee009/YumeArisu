using System.Numerics;
using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Internal.RenderPipeline;

/// <summary>
/// Sprite 렌더러가 소유하는 per-object 데이터입니다.
/// 오직 이 struct를 들고 있는 Renderer 내부에서만 값을 채우고 Apply를 호출합니다.
/// </summary>
internal struct SpriteObjectData
{
    internal Matrix4x4 Model;
    internal Vector2 SpriteSize;
    internal Vector2 SpritePivot;
    internal Vector4 UVRect;
    internal Vector4 Color;
    internal Texture MainTexture;

    /// <summary>
    /// 고정된 이름으로 셰이더에 직접 업로드합니다.
    /// per-material(Material/MaterialOverride) 병합과는 완전히 분리된 경로입니다.
    /// </summary>
    /// <param name="textureUnit">Material.Apply가 이미 사용한 다음 여유 텍스쳐 유닛</param>
    internal readonly void Apply(Shader shader, int textureUnit)
    {
        shader.SetMatrix4x4("Model", Model);
        shader.SetVector2("SpriteSize", SpriteSize.X, SpriteSize.Y);
        shader.SetVector2("SpritePivot", SpritePivot.X, SpritePivot.Y);
        shader.SetVector4("UVRect", UVRect.X, UVRect.Y, UVRect.Z, UVRect.W);
        shader.SetVector4("Color", Color.X, Color.Y, Color.Z, Color.W);
        shader.SetTexture("MainTexture", MainTexture, textureUnit);
    }
}
