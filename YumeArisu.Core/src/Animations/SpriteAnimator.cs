using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Utility;
using YumeArisu.Core.Rendering;

namespace YumeArisu.Core.Animations;

/// <summary>
/// 같은 GameObject의 SpriteRenderer.Sprite를 프레임 단위로 교체합니다.
/// 클립은 공유 리소스이므로 읽기만 하고 소유하지 않습니다.
/// </summary>
public class SpriteAnimator : Behaviour
{
    /// <summary>일반 스크립트(0)가 끝난 뒤 실행되도록 늦춥니다.</summary>
    public override int ExecutionOrder => ReservedExecutionOrder.Animation;

    /// <summary>대입하면 재생 중이던 것은 멈추고 첫 프레임을 표시합니다.</summary>
    public SpriteAnimationClip Clip
    {
        get => _clip;
        set => SetClip(value);
    }

    public float Speed { get => _speed; set => _speed = MathF.Max(0f, value); }
    public bool UseUnscaledTime { get; set; }
    public bool PlayOnStart { get; set; } = true;
    public bool IsPlaying { get; private set; }
    public int FrameIndex => _frame;

    /// <summary>Once 클립이 마지막 프레임까지 재생되었을 때 호출됩니다.</summary>
    public event Action Finished;

    /// <summary>Loop 클립이 첫 프레임으로 돌아올 때 호출됩니다.</summary>
    public event Action Looped;

    /// <summary>프레임에 진입할 때마다 호출됩니다. 인자는 프레임 인덱스입니다.</summary>
    public event Action<int> FrameChanged;

    /// <summary>클립의 프레임 Event에 진입할 때 호출됩니다. 인자는 이벤트 이름입니다.</summary>
    public event Action<string> FrameEvent;

    private readonly Dictionary<string, SpriteAnimationClip> _clips = new();
    private SpriteAnimationClip _clip;
    private SpriteRenderer _renderer;
    private float _speed = 1f;
    private float _timeInFrame;
    private int _frame;
    private int _direction = 1;
    private int _version; // Play/Stop/SetClip마다 증가 - 이벤트 핸들러 재진입 감지용
    private bool _warnedNoRenderer;

    public override void OnStart()
    {
        if (PlayOnStart && _clip is not null && !IsPlaying)
            Play();
    }

    public override void OnUpdate()
    {
        if (!IsPlaying || _clip is null)
            return;

        _timeInFrame += (UseUnscaledTime ? Time.UnscaledDeltaTime : Time.DeltaTime) * _speed;

        // 프레임레이트가 낮아도 프레임과 이벤트를 건너뛰지 않도록 while로 넘긴다
        int version = _version;
        while (IsPlaying && _timeInFrame >= _clip.GetDuration(_frame))
        {
            _timeInFrame -= _clip.GetDuration(_frame);
            Advance();

            // 핸들러가 Play/Stop을 호출했다면 이번 프레임 진행은 중단
            if (version != _version)
                return;
        }
    }

    protected internal override void OnDetach()
    {
        base.OnDetach();

        _clips.Clear();
        _renderer = null;
        Finished = null;
        Looped = null;
        FrameChanged = null;
        FrameEvent = null;
    }

    /// <summary>Play(string)으로 재생할 수 있도록 클립에 이름을 붙여 등록합니다.</summary>
    public void AddClip(string name, SpriteAnimationClip clip)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(clip);

        _clips[name] = clip;
    }

    /// <summary>현재 클립을 처음부터 재생합니다.</summary>
    public void Play()
    {
        if (_clip is null)
            return;

        _version++;
        _direction = 1;
        _timeInFrame = 0f;
        IsPlaying = true;
        EnterFrame(0);
    }

    /// <param name="restart">false면 같은 클립을 재생 중일 때 무시합니다. (매 프레임 호출해도 안전)</param>
    public void Play(SpriteAnimationClip clip, bool restart = false)
    {
        ArgumentNullException.ThrowIfNull(clip);

        if (!restart && IsPlaying && _clip == clip)
            return;

        _clip = clip;
        Play();
    }

    /// <param name="name">AddClip으로 등록한 이름</param>
    public void Play(string name, bool restart = false)
    {
        if (!_clips.TryGetValue(name, out var clip))
            throw new InvalidOperationException($"등록되지 않은 클립입니다: {name}");

        Play(clip, restart);
    }

    /// <summary>재생을 멈추고 첫 프레임으로 되돌립니다.</summary>
    public void Stop()
    {
        if (_clip is null)
            return;

        _version++;
        IsPlaying = false;
        _direction = 1;
        _timeInFrame = 0f;
        _frame = 0;
        ApplySprite();
    }

    public void Pause() => IsPlaying = false;
    public void Resume() => IsPlaying = _clip is not null;

    private void SetClip(SpriteAnimationClip clip)
    {
        _clip = clip;
        _version++;
        IsPlaying = false;
        _direction = 1;
        _timeInFrame = 0f;
        _frame = 0;

        if (_clip is not null)
            ApplySprite(); // 이벤트 없이 첫 프레임만 표시
    }

    /// <summary>현재 프레임의 지속시간이 끝났을 때 다음 프레임을 결정합니다.</summary>
    private void Advance()
    {
        int last = _clip.FrameCount - 1;
        int next = _frame + _direction;
        bool looped = false;

        switch (_clip.Mode)
        {
            case SpriteAnimationMode.Once:
                if (next > last)
                {
                    // 마지막 프레임을 유지한 채 종료
                    IsPlaying = false;
                    _timeInFrame = 0f;
                    Finished?.Invoke();
                    return;
                }
                break;

            case SpriteAnimationMode.Loop:
                if (next > last)
                {
                    next = 0;
                    looped = true;
                }
                break;

            case SpriteAnimationMode.PingPong:
                if (next > last)
                {
                    _direction = -1;
                    next = Math.Max(last - 1, 0);
                }
                else if (next < 0)
                {
                    _direction = 1;
                    next = Math.Min(1, last);
                }
                break;
        }

        EnterFrame(next);

        if (looped)
            Looped?.Invoke();
    }

    private void EnterFrame(int index)
    {
        _frame = index;
        ApplySprite();
        FrameChanged?.Invoke(index);

        var eventName = _clip.GetEvent(index);
        if (!string.IsNullOrEmpty(eventName))
            FrameEvent?.Invoke(eventName);
    }

    private void ApplySprite()
    {
        var renderer = ResolveRenderer();
        if (renderer is not null)
            renderer.Sprite = _clip.GetSprite(_frame);
    }

    private SpriteRenderer ResolveRenderer()
    {
        _renderer ??= GetComponent<SpriteRenderer>();

        if (_renderer is null && !_warnedNoRenderer)
        {
            _warnedNoRenderer = true;
            ConsoleExtensions.WriteLineColored(
                $"[SpriteAnimator] '{GameObject.Name}'에 SpriteRenderer가 없습니다.", ConsoleColor.Yellow);
        }

        return _renderer;
    }
}