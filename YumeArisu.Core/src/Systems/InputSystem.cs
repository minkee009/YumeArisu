using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Input;
using Silk.NET.Windowing;
using YumeArisu.Core.Internal.InputHandling;

namespace YumeArisu.Core.Systems;

public class InputSystem : SystemBase<InputSystem, IView>
{
    private KeyboardState _keyboardState;
    private MouseState _mouseState;
    private IInputContext _input;

    public override void StartUpInternal(IView view)
    {
        _input = view.CreateInput();
        _keyboardState = new KeyboardState(_input.Keyboards[0]);
        _mouseState = new MouseState(_input.Mice[0]);
        _input.ConnectionChanged += DoConnect;
    }

    public override void ShutDownInternal() => _input?.Dispose();

    public void DoConnect(IInputDevice device, bool connected)
    {
        switch (device)
        {
            case IKeyboard:
                if (_input.Keyboards.Count > 0)
                    _keyboardState.ConnectionChanged(_input.Keyboards[0]);
                else
                    _keyboardState.Reset();
                break;

            case IMouse:
                if (_input.Mice.Count > 0)
                    _mouseState.ConnectionChanged(_input.Mice[0]);
                else
                    _mouseState.Reset();
                break;
        }
    }

    public void EndFrame()
    {
        _keyboardState?.EndFrame();
        _mouseState?.EndFrame();
    }

    public bool GetKey(Key key) => (_keyboardState.Current[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;
    public bool GetKeyDown(Key key) => (_keyboardState.Pressed[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;
    public bool GetKeyUp(Key key) => (_keyboardState.Released[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;

    public Vector2 GetMousePosition() => _mouseState.Position;
    public Vector2 GetMouseDelta() => _mouseState.Delta;
    public bool GetMouseButton(MouseButton button) => (_mouseState!.ButtonCurrent & (1 << (int)button)) != 0;
    public bool GetMouseButtonDown(MouseButton button) => (_mouseState!.ButtonPressed & (1 << (int)button)) != 0;
    public bool GetMouseButtonUp(MouseButton button) => (_mouseState!.ButtonReleased & (1 << (int)button)) != 0;
}

// 문법 설탕용 클래스
public static class Input
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetKey(Key key) => InputSystem.Instance.GetKey(key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetKeyDown(Key key) => InputSystem.Instance.GetKeyDown(key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetKeyUp(Key key) => InputSystem.Instance.GetKeyUp(key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GetMousePosition() => InputSystem.Instance.GetMousePosition();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GetMouseDelta() => InputSystem.Instance.GetMouseDelta();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetMouseButton(MouseButton button) => InputSystem.Instance.GetMouseButton(button);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetMouseButtonDown(MouseButton button) => InputSystem.Instance.GetMouseButtonDown(button);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetMouseButtonUp(MouseButton button) => InputSystem.Instance.GetMouseButtonUp(button);
}