namespace YumeAris.Core.System;

public class TimeSystem : SystemBase<TimeSystem, NoConfig >
{
    public float DeltaTime { get => (float)_deltaTime; }
    public float UnscaledDeltaTime { get => (float)_unscaledDeltaTime; }
    public float TotalTime { get => (float)_totalTime; }
    public float UnscaledTotalTime { get => (float)_unscaledTotalTime; }
    public float FixedDeltaTime { get => (float)_fixedDeltaTime; }
    public float FixedTime { get => (float)_fixedTime; }
    public float TimeScale { get => (float)_timeScale; }
    public float MaximumAllowedTimestep { get => (float)_maximumAllowedTimestep; }
    public float InterpolationAlpha { get => (float)_interpolationAlpha; }
    public uint FrameCount { get; private set; }

    public double HighResDeltaTime { get => _deltaTime; }
    public double HighResTotalTime { get => _totalTime; }
    public double HighResUnscaledDeltaTime { get=> _unscaledDeltaTime; }
    public double HighResUnscaledTotalTime { get => _unscaledTotalTime; }
    public double HighResFixedDeltaTime { get => _fixedDeltaTime; }
    public double HighResFixedTime { get => _fixedTime; }

    private double _deltaTime;
    private double _unscaledDeltaTime;
    private double _totalTime;
    private double _unscaledTotalTime;
    private double _fixedDeltaTime = 0.03125;
    private double _fixedTime;
    private double _timeScale = 1.0;
    private double _maximumAllowedTimestep = 0.2;
    private double _interpolationAlpha;

    private double _accumulator;
    private int _fixedStepsThisFrame;

    public override void StartUpInternal(NoConfig config)
    {
        _deltaTime = 0.0;
        _unscaledDeltaTime = 0.0;
        _totalTime = 0.0;
        _unscaledTotalTime = 0.0;
        _fixedDeltaTime = 0.03125;
        _fixedTime = 0.0;
        _timeScale = 1.0;
        _maximumAllowedTimestep = 0.2;
        _interpolationAlpha = 0.0;
        FrameCount = 0;

        _accumulator = 0.0;
        _fixedStepsThisFrame = 0;
    }

    public override void ShutDownInternal()
    {
        
    }

    /// <summary>
    /// TimeSystem 내부의 변수값을 갱신합니다.
    /// </summary>
    /// <param name="deltaTime">한 프레임이 걸린 시간입니다. 고정밀 타이머를 통해 측정된 값이어야 합니다.</param>
    public void BeginFrame(double deltaTime)
    {
        // 프레임 카운트 측정
        FrameCount++;

        // 큰 델타타임 자르기
        _unscaledDeltaTime = Math.Min(deltaTime, _maximumAllowedTimestep);

        // 스케일링된 델타타임 계산 + 누적
        _deltaTime = _unscaledDeltaTime * _timeScale;
        _unscaledTotalTime += _unscaledDeltaTime;
        _totalTime += _deltaTime;

        // 고정 스텝 수 측정
        _accumulator += _deltaTime;
        _fixedStepsThisFrame = 0;
        while (_accumulator >= _fixedDeltaTime)
        {
            _accumulator -= _fixedDeltaTime;
            _fixedStepsThisFrame++;
        }

        _interpolationAlpha = _fixedDeltaTime > 0
            ? _accumulator / _fixedDeltaTime
            : 0;    
    }

    /// <summary>
    /// BeginFrame 에서 측정된 고정 스텝을 처리합니다.
    /// 호출 시 누적된 모든 고정 스텝을 소모하여 처리하고, 인자로 받아온 외부 함수를 실행합니다.
    /// </summary>
    /// <param name="step"></param>
    public void ConsumeFixedSteps(Action<float> step)
    {
        for (int i = 0; i < _fixedStepsThisFrame; i++)
        {
            _fixedTime += _fixedDeltaTime;
            step((float)_fixedDeltaTime);
        }
    }

    public void SetTimeScale(float timeScale) => _timeScale = Math.Max(0.0, timeScale);
    public void SetFixedDeltaTime(float fixedDelta) => _fixedDeltaTime = Math.Max(0.0001, fixedDelta);
    public void SetMaximumAllowedTimestep(float allowedTimestep) => _maximumAllowedTimestep = Math.Max(0.0001, allowedTimestep);
    public void ResetFrameCount() => FrameCount = 0;
}

// 문법 설탕용 클래스
static class Time
{
    public static float DeltaTime => TimeSystem.Instance.DeltaTime;
    public static float UnscaledDeltaTime => TimeSystem.Instance.UnscaledDeltaTime;
    public static float TotalTime => TimeSystem.Instance.TotalTime;
    public static float UnscaledTotalTime => TimeSystem.Instance.UnscaledTotalTime;
    public static float FixedDeltaTime => TimeSystem.Instance.FixedDeltaTime;
    public static float FixedTime => TimeSystem.Instance.FixedTime;
    public static float TimeScale => TimeSystem.Instance.TimeScale;
    public static float MaximumAllowedTimestep => TimeSystem.Instance.MaximumAllowedTimestep;
    public static float InterpolationAlpha => TimeSystem.Instance.InterpolationAlpha;
    public static uint FrameCount => TimeSystem.Instance.FrameCount;
    public static double HighResDeltaTime => TimeSystem.Instance.HighResDeltaTime;
    public static double HighResTotalTime => TimeSystem.Instance.HighResTotalTime;
    public static double HighResUnscaledDeltaTime => TimeSystem.Instance.HighResUnscaledDeltaTime;
    public static double HighResUnscaledTotalTime => TimeSystem.Instance.HighResUnscaledTotalTime;
    public static double HighResFixedDeltaTime => TimeSystem.Instance.HighResFixedDeltaTime;
    public static double HighResFixedTime => TimeSystem.Instance.HighResFixedTime;
}
