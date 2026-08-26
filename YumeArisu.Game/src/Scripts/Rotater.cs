using System.Collections;
using System.Numerics;
using Silk.NET.Input;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;

namespace YumeArisu.Game.Scripts;

public class Rotater : ScriptBehaviour
{
    
    public float RotateSpeed { get => _rotateSpeed; set =>  _rotateSpeed = value < float.Epsilon ? 0.0f : value / 57.2957795f; }
    float _rot = 0.0f;
    float _rotateSpeed = 45.0f;
    public override void OnUpdate()
    {
        _rot += RotateSpeed * Time.DeltaTime;
        Transform.LocalRotation = Quaternion.CreateFromYawPitchRoll(0,0, _rot);
    }
}