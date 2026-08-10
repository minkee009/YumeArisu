using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Internal.InputHandling;
using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Systems;

public class InputSystem : SystemBase<InputSystem, IView>
{
    private KeyboardState _keyboardState;
    private MouseState _mouseState;
    private IInputContext _input;
    private List<(List<Key> alternativeKeys, Key triggerKey, Action action)> _systemKeyCombos;

    internal override void OnStartUp(IView view)
    {
        _input = view.CreateInput();
        _keyboardState = new KeyboardState(_input.Keyboards[0]);
        _mouseState = new MouseState(_input.Mice[0]);
        _input.ConnectionChanged += DoConnect;
        _systemKeyCombos = new();
    }

    internal override void OnShutDown()
    {
        _input?.Dispose();
        _systemKeyCombos?.Clear();

        _input = null;
        _systemKeyCombos = null;
    }

    public void OnResize(Vector2D<int> size)
    {
        _mouseState.ViewSize = size.ToNumerics();
    }

    public void DoConnect(IInputDevice device, bool connected)
    {
        switch (device)
        {
            case IKeyboard:
                if (_input.Keyboards.Count > 0)
                    _keyboardState.ConnectionChanged(_input.Keyboards[0]);  
                else
                    _keyboardState.Reset();

                RefreshSystemKeyCombos();
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

    public IInputContext GetInputContext() => _input;

    /// <summary>
    /// holdKeys가 모두 눌린 상태에서 triggerKey가 눌리는 순간 action을 실행합니다.
    /// </summary>
    public void RegisterSystemKeyCombo(List<Key> holdKeys, Key triggerKey, Action action)
    {
        _systemKeyCombos.Add((holdKeys, triggerKey, action));
        RefreshSystemKeyCombos();
    }

    /// <summary>
    /// 키보드 이벤트에서 시스템 키콤보에 대해 구독을 갱신합니다. (키보드 연결/해제 시 호출)
    /// </summary>
    private void RefreshSystemKeyCombos()
    {
        if (_input.Keyboards.Count == 0)
            return;

        var keyboard = _input.Keyboards[0];
        keyboard.KeyDown -= OnSystemKeyComboKeyDown; // 이미 구독돼 있으면 먼저 제거 (중복 방지)
        keyboard.KeyDown += OnSystemKeyComboKeyDown;
    }

    private void OnSystemKeyComboKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        foreach (var (holdKeys, triggerKey, action) in _systemKeyCombos)
        {
            if (key == triggerKey && holdKeys.All(k => keyboard.IsKeyPressed(k)))
                action();
        }
    }
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