using System.Collections;
using Silk.NET.Input;
using Silk.NET.Windowing;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;

public class FPSChecker : ScriptBehaviour
{
    private float[] _counts = new float[5];
    private int _index = 0;
    private float _tickrate = 0.2f;
    private float _timer = 0.0f;

    public override void Start()
    {
        Array.Clear(_counts);
    }

    public override void Update()
    {
        if (_timer < _tickrate)
        {
            _timer += Time.DeltaTime;
            return;
        }
        _timer = 0.0f;
        _counts[_index] = 1.0f / Time.DeltaTime;
        _index++;
        _index %= _counts.Length;

        var total = 0f;
        foreach(var c in _counts)
        {
            total += c;
        }

        var average = (int)(total / _counts.Length);

        WindowControl.SetTitle($"fps - {average}");
    }
}