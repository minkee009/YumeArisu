using System.Collections;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;

public class WindowMover : ScriptBehaviour
{
    public float Speed { get; set; } = 250.0f;
    Vector2 _velocity;
    Vector2 _localPosition;
    //public bool _isGrounded = false;

    public override void Start()
    {
        var currentPos = WindowControl.Position;
        _localPosition = new(currentPos.X, currentPos.Y);
    }
    
    public override void Update()
    {
        var hInput = (Input.GetKey(Key.Right) ? 1.0f : 0.0f) + (Input.GetKey(Key.Left) ? -1.0f : 0.0f);
        var vInput = (Input.GetKey(Key.Down) ? 1.0f : 0.0f) + (Input.GetKey(Key.Up) ? -1.0f : 0.0f);

        var targetVelocity = new Vector2(hInput,vInput) * Speed * Time.DeltaTime;
        _velocity = Vector2.Lerp(_velocity, targetVelocity, Time.DeltaTime * 2.0f);
        
        // if (_isGrounded && Input.GetKeyDown(Key.Space))
        // {
        //     _isGrounded = false;
        //     _velocity += new Vector2(0.0f, -0.5f);
        // }

        // if (!_isGrounded && _localPosition.Y < 1080 - WindowControl.GetSize().Y)
        // {
        //     _localPosition += new Vector2(0.0f, 600.0f) * Time.DeltaTime;
        //     if (_localPosition.Y >= 1080 - WindowControl.GetSize().Y)
        //     {
        //         _isGrounded = true;
        //         _localPosition.Y = 1081 - WindowControl.GetSize().Y;
        //         _velocity.Y = 0.0f;
        //     }
        // }

        _localPosition += _velocity;

        WindowControl.Position = new Vector2D<int>((int)_localPosition.X, (int)_localPosition.Y);
    }
}