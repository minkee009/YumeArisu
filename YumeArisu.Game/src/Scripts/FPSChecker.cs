using System.Collections;
using Silk.NET.Input;
using Silk.NET.Windowing;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;

namespace YumeArisu.Game.Scripts;

public class FPSChecker : ScriptBehaviour
{
    private string _currentTitle = "";
    private float _accumulatedTime = 0f;
    private int _frameCount = 0;
    private float _updateInterval = 0.15f;

    public override void OnStart()
    {
        _currentTitle = WindowControl.Title;
        //ApplicationControl.VSync = false;
    }

    public override void OnUpdate()
    {
        _accumulatedTime += Time.DeltaTime;
        _frameCount++;

        if (_accumulatedTime >= _updateInterval)
        {
            float fps = _frameCount / _accumulatedTime;

            WindowControl.Title = $"{_currentTitle} | fps - {(int)fps}";

            _accumulatedTime = 0f;
            _frameCount = 0;
        }
    }
}