using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Utility;

namespace YumeArisu.Desktop.Implements;

using DesktopKeyboard = Internal.KeyboardState;
using DesktopMouse = Internal.MouseState;

public class DesktopInputDevice : IInputSource
{
    public IInputContext GetInputContext() => _input;

    private IInputContext _input;
    private DesktopKeyboard _keyboard;
    private DesktopMouse _mouse;

    private List<(List<Key> alternativeKeys, Key triggerKey, Action action)> _systemKeyCombos;

    public bool UpdateMustEndOfFrame => true;

    public DesktopInputDevice(IView view)
    {
        _systemKeyCombos = new();

        _input = view.CreateInput();
        _input.ConnectionChanged += DoConnect;
        _keyboard = new DesktopKeyboard(_input.Keyboards[0]);
        _mouse = new DesktopMouse(_input.Mice[0]);
    }

    public void DoConnect(IInputDevice device, bool connected)
    {
        switch (device)
        {
            case IKeyboard:
                if (_input.Keyboards.Count > 0)
                    _keyboard.ConnectionChanged(_input.Keyboards[0]);  
                else
                    _keyboard.Reset();

                RefreshSystemKeyCombos();
                break;

            case IMouse:
                if (_input.Mice.Count > 0)
                    _mouse.ConnectionChanged(_input.Mice[0]);
                else
                    _mouse.Reset();
                break;
        }
    }

    public bool GetKey(Key key) => (_keyboard.Current[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;
    public bool GetKeyDown(Key key) => (_keyboard.Pressed[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;
    public bool GetKeyUp(Key key) => (_keyboard.Released[(int)key >> 6] & (1UL << ((int)key & 63))) != 0;

    public Vector2 GetMousePosition() => _mouse.Position;
    public Vector2 GetMouseDelta() => _mouse.Delta;
    public bool GetMouseButton(MouseButton button) => (_mouse!.ButtonCurrent & (1 << (int)button)) != 0;
    public bool GetMouseButtonDown(MouseButton button) => (_mouse!.ButtonPressed & (1 << (int)button)) != 0;
    public bool GetMouseButtonUp(MouseButton button) => (_mouse!.ButtonReleased & (1 << (int)button)) != 0;

    public void OnViewResize(Vector2D<int> size)
    {
        _mouse.ViewSize = size.ToNumerics();
    }

    public void Update()
    {
        _keyboard?.EndFrame();
        _mouse?.EndFrame();
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