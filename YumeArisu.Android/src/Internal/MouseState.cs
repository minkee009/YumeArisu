using Silk.NET.Input;
using System.Numerics;

namespace YumeArisu.Android.Internal;

internal class MouseState
{   
    public Vector2 Position { get; private set; }
    public Vector2 Delta { get; private set; }
    public float Scroll { get; private set; }

    public int ButtonPressed { get; private set; }
    public int ButtonReleased { get; private set; }
    public int ButtonCurrent { get; private set; }

    internal Vector2 DeltaAccum { get; private set; }
    internal Vector2 ViewSize { get; set; }

    private IMouse _mouse;
    
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

    private void OnButtonDown(IMouse mouse, MouseButton button)
    {
        int b = (int)button;
        ButtonPressed |= 1 << b;
        ButtonCurrent |= 1 << b;
    }

    private void OnButtonUp(IMouse mouse, MouseButton button)
    {
        int b = (int)button;
        ButtonReleased |= 1 << b;
        ButtonCurrent &= ~(1 << b);
    }

    private void OnScroll(IMouse mouse, ScrollWheel scroll)
    {
        Scroll += scroll.Y;
    }

    private void OnMove(IMouse mouse, Vector2 position)
    {
        var accurateGlPos = position;
        accurateGlPos.Y = -(accurateGlPos.Y - ViewSize.Y);

        DeltaAccum += accurateGlPos - Position;
        Position = accurateGlPos;
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
