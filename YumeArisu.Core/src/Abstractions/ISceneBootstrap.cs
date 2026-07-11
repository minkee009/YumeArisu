using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Core.Abstractions;

/// <summary>
/// 진입 씬과 사용할 씬들에 대한 묶음을 정의합니다.
/// DynamicScenes의 첫번째 씬이 Entry로 지정됩니다.
/// </summary>
public interface ISceneManifest
{
    Scene[] DynamicScenes { get; }
    Scene[] StaticScenes { get; }
}