using System.Collections;
using System.Numerics;
using System.Runtime;
using Silk.NET.Input;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;

namespace YumeArisu.Game.Scripts;

public class CamMover : ScriptBehaviour
{
    float _movespeed = 5.0f;
    float _rotspeed = 0.0025f;
    float _targetPitch = 0.0f;
    float _targetYaw = 0.0f;

    float _pitch = 0.0f;
    float _yaw = 0.0f;

    Vector3 _targetVelocity = Vector3.Zero;

    Vector3 _velocity = Vector3.Zero;

    public override void OnUpdate()
    {
        float xInput = (Input.GetKey(Key.D) ? 1 : 0) + (Input.GetKey(Key.A) ? -1 : 0);
        float zInput = (Input.GetKey(Key.S) ? 1 : 0) + (Input.GetKey(Key.W) ? -1 : 0);
        float yInput = (Input.GetKey(Key.E) ? 1 : 0) + (Input.GetKey(Key.Q) ? -1 : 0);

        if (Input.GetMouseButton(MouseButton.Right))
        {
            var delta = Input.GetMouseDelta();
            if (delta != Vector2.Zero)
            {
                _targetYaw -= delta.X * _rotspeed;
                _targetPitch += delta.Y * _rotspeed;
            }
        }

        _pitch = float.Lerp(_pitch,_targetPitch,12.0f * Time.DeltaTime);
        _yaw = float.Lerp(_yaw,_targetYaw,12.0f * Time.DeltaTime);

        _targetVelocity = new Vector3(xInput, yInput, zInput);
        if(_targetVelocity.LengthSquared() > float.Epsilon) _targetVelocity = Vector3.Normalize(_targetVelocity);
        _targetVelocity = Vector3.Transform(_targetVelocity,Transform.LocalRotation) *  _movespeed;
        
        if (Input.GetKey(Key.ShiftLeft)) _targetVelocity *= 3.0f;
        _velocity = Vector3.Lerp(_velocity, _targetVelocity , 6.0f * Time.DeltaTime);

        Transform.LocalRotation = Quaternion.CreateFromYawPitchRoll(_yaw,_pitch,0);
        Transform.LocalPosition += _velocity * Time.DeltaTime; 
    }
}