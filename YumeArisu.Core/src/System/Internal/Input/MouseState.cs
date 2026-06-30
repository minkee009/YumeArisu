using Silk.NET.Input;
using Silk.NET.Maths;
using System.Numerics;

namespace YumeAris.Core.System.Internal.Input;

internal class MouseState
{   
    public Vector2 Position { get; private set; }
    public Vector2 Delta { get; private set; }
    public Vector2 DeltaAccum { get; private set; }
    public float Scroll { get; private set; }

    public int ButtonPressed { get; private set; }
    public int ButtonReleased { get; private set; }
    public int ButtonCurrent { get; private set; }

    IMouse _mouse;
    
    public MouseState(IMouse mouse)
    {
        _mouse = mouse;
        Subscribe(_mouse);
        Reset();
    }

    public void EndFrame()
    {
        ButtonPressed = 0;
        ButtonReleased = 0;
        Scroll = 0;
        Delta = DeltaAccum;
        DeltaAccum = Vector2.Zero;
    }

    public void ConnectionChanged(IMouse mouse)
    {
        Unsubscribe(_mouse);
        _mouse = mouse;
        Subscribe(_mouse);
        Reset();
    }

    public void OnButtonDown(IMouse mouse, MouseButton button)
    {
        int b = (int)button;
        ButtonPressed |= 1 << b;
        ButtonCurrent |= 1 << b;
    }

    public void OnButtonUp(IMouse mouse, MouseButton button)
    {
        int b = (int)button;
        ButtonReleased |= 1 << b;
        ButtonCurrent &= ~(1 << b);
    }

    public void OnScroll(IMouse mouse, ScrollWheel scroll)
    {
        Scroll += scroll.Y;
    }

    public void OnMove(IMouse mouse, Vector2 position)
    {
        DeltaAccum += position - Position;
        Position = position;
    }

    internal void Reset()
    {
        ButtonPressed = 0;
        ButtonReleased = 0;
        ButtonCurrent = 0;
        Position = _mouse.Position;
        Delta = Vector2.Zero;
        DeltaAccum = Vector2.Zero;
        Scroll = 0;
    }

    public void Subscribe(IMouse mouse)
    {
        mouse.MouseDown += OnButtonDown;
        mouse.MouseUp += OnButtonUp;
        mouse.Scroll += OnScroll;
        mouse.MouseMove += OnMove;
    }

    public void Unsubscribe(IMouse mouse)
    {
        mouse.MouseDown -= OnButtonDown;
        mouse.MouseUp -= OnButtonUp;
        mouse.Scroll -= OnScroll;
        mouse.MouseMove -= OnMove;
    }
}
