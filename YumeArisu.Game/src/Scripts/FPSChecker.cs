using System.Collections;
using Silk.NET.Input;
using Silk.NET.Windowing;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;

public class FPSChecker : ScriptBehaviour
{
    private string _currentTitle = "";
    private float _accumulatedTime = 0f;
    private int _frameCount = 0;
    private float _updateInterval = 0.5f;

    public override void Start()
    {
        _currentTitle = WindowControl.GetTitle();
        Application.SetVSync(false);
    }

    public override void Update()
    {
        _accumulatedTime += Time.DeltaTime;
        _frameCount++;

        if (_accumulatedTime >= _updateInterval)
        {
            float fps = _frameCount / _accumulatedTime;

            WindowControl.SetTitle($"{_currentTitle} | fps - {(int)fps}");

            _accumulatedTime = 0f;
            _frameCount = 0;
        }
    }
}