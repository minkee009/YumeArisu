using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Utility;

namespace YumeArisu.Android.Implements;

public class AndroidInputSource : IInputSource
{
    private IInputContext _input;
    private KeyboardState _keyboardState;
    private MouseState _mouseState;

    private List<(List<Key> alternativeKeys, Key triggerKey, Action action)> _systemKeyCombos;

    public bool UpdateMustEndOfFrame => true;

    public void Initialize(IView view)
    {
        _systemKeyCombos = new();

        _input = view.CreateInput();
        _input.ConnectionChanged += DoConnect;
        _keyboardState = new KeyboardState(_input.Keyboards[0]);
        _mouseState = new MouseState(_input.Mice[0]);
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

    public bool GetKey(Key key) => (_keyboardState.Current[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;
    public bool GetKeyDown(Key key) => (_keyboardState.Pressed[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;
    public bool GetKeyUp(Key key) => (_keyboardState.Released[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;

    public Vector2 GetMousePosition() => _mouseState.Position;
    public Vector2 GetMouseDelta() => _mouseState.Delta;
    public bool GetMouseButton(MouseButton button) => (_mouseState!.ButtonCurrent & (1 << (int)button)) != 0;
    public bool GetMouseButtonDown(MouseButton button) => (_mouseState!.ButtonPressed & (1 << (int)button)) != 0;
    public bool GetMouseButtonUp(MouseButton button) => (_mouseState!.ButtonReleased & (1 << (int)button)) != 0;

    public void OnViewResize(Vector2D<int> size)
    {
        _mouseState.ViewSize = size.ToNumerics();
    }

    public void Update()
    {
        _keyboardState?.EndFrame();
        _mouseState?.EndFrame();
    }

    public void RegisterSystemKeyCombo(List<Key> holdKeys, Key triggerKey, Action action)
    {
        _systemKeyCombos.Add((holdKeys, triggerKey, action));
        RefreshSystemKeyCombos();
    }

    public void RefreshSystemKeyCombos()
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

    public void Dispose()
    {
        _input?.Dispose();
        _systemKeyCombos?.Clear();
        _systemKeyCombos = null;
    }
}