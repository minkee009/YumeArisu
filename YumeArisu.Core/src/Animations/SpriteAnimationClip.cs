using System.Text;
using YumeArisu.Core.Internal.AnimationPipeline;
using YumeArisu.Core.Internal.ResourceHandling;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Animations;

public class SpriteAnimationClip : Resource
{
    public float FrameRate { get; private set; }
    public SpriteAnimationMode Mode { get; private set; }

    /// <summary>한 방향으로 전부 재생하는 데 걸리는 시간(초)입니다.</summary>
    public float Length { get; private set; }

    public int FrameCount => _sprites?.Length ?? 0;

    private Sprite[] _sprites;
    private float[] _durations; // 초 단위
    private string[] _events;

    public Sprite GetSprite(int index) => _sprites[index];
    public float GetDuration(int index) => _durations[index];
    public string GetEvent(int index) => _events[index];

    /// <summary>
    /// 파일 없이 코드에서 클립을 만듭니다. 리소스 시스템 소유가 아니므로 Sprite 해제는 호출한 쪽 책임입니다.
    /// </summary>
    public static SpriteAnimationClip FromSprites(
        IReadOnlyList<Sprite> sprites, float frameRate, SpriteAnimationMode mode = SpriteAnimationMode.Loop)
    {
        if (sprites is null || sprites.Count == 0)
            throw new ArgumentException("스프라이트가 하나 이상 필요합니다.", nameof(sprites));

        if (frameRate <= 0f)
            throw new ArgumentOutOfRangeException(nameof(frameRate));

        var durations = new float[sprites.Count];
        Array.Fill(durations, 1f / frameRate);

        var clip = new SpriteAnimationClip();
        clip.Setup(sprites.ToArray(), durations, new string[sprites.Count], frameRate, mode);
        clip.IsLoaded = true;
        return clip;
    }

    protected override bool OnLoad(byte[] bytes)
    {
        if (!PathHelper.HasExtension(Path, ".spriteanim"))
            return false;

        var meta = JsonMetaParser.Parse<SpriteAnimationClipMeta>(Encoding.UTF8.GetString(bytes));

        if (meta.Frames is null || meta.Frames.Length == 0)
            throw new InvalidDataException($"프레임이 없는 스프라이트 애니메이션입니다: {Path}");

        if (meta.FrameRate <= 0f)
            throw new InvalidDataException($"FrameRate는 0보다 커야 합니다: {Path}");

        var mode = SpriteAnimationMode.Loop;
        if (!string.IsNullOrEmpty(meta.Mode) && !Enum.TryParse(meta.Mode, true, out mode))
            throw new InvalidDataException($"알 수 없는 Mode: {meta.Mode} ({Path})");

        int count = meta.Frames.Length;
        var sprites = new List<Sprite>(count);
        var durations = new float[count];
        var events = new string[count];

        try
        {
            for (int i = 0; i < count; i++)
            {
                var frame = meta.Frames[i];

                sprites.Add(Resources.Get<Sprite>(frame.Sprite)); // 프레임마다 refCount +1
                durations[i] = (frame.Duration is > 0f ? frame.Duration.Value : 1f) / meta.FrameRate;
                events[i] = frame.Event;
            }
        }
        catch
        {
            foreach (var sprite in sprites)
                Resources.Release(sprite);

            throw;
        }

        Setup(sprites.ToArray(), durations, events, meta.FrameRate, mode);
        return true;
    }

    protected override void OnUnload()
    {
        if (IsLoadedBySystem && _sprites is not null)
        {
            foreach (var sprite in _sprites)
                Resources.Release(sprite); // Get한 횟수와 동일하게 반납
        }

        _sprites = null;
        _durations = null;
        _events = null;
        FrameRate = 0f;
        Length = 0f;
        Mode = default;
    }

    private void Setup(Sprite[] sprites, float[] durations, string[] events, float frameRate, SpriteAnimationMode mode)
    {
        _sprites = sprites;
        _durations = durations;
        _events = events;
        FrameRate = frameRate;
        Mode = mode;
        Length = durations.Sum();
    }
}