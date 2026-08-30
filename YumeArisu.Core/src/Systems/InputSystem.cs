using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Utility;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Core.Systems;

public class InputSystem : SystemBase<InputSystem, IInputSource>
{
    private IInputSource _input;

    internal override void OnStartUp(IInputSource inputSource) => _input = inputSource;

    internal override void OnShutDown()
    {
        _input?.Dispose();
        _input = null;
    }

    public void OnViewResize(Vector2D<int> size) => _input.OnViewResize(size);

    public void BeginFrame()
    {
        if(!_input.UpdateMustEndOfFrame)
            _input.Update();
    }

    public void EndFrame()
    {
        if(_input.UpdateMustEndOfFrame)
            _input.Update();
    }

    public bool GetKey(Key key) => _input.GetKey(key);
    public bool GetKeyDown(Key key) => _input.GetKeyDown(key);
    public bool GetKeyUp(Key key) => _input.GetKeyUp(key);

    public Vector2 GetMousePosition() => _input.GetMousePosition();
    public Vector2 GetMouseDelta() => _input.GetMouseDelta();
    public bool GetMouseButton(MouseButton button) => _input.GetMouseButton(button);
    public bool GetMouseButtonDown(MouseButton button) => _input.GetMouseButtonDown(button);
    public bool GetMouseButtonUp(MouseButton button) => _input.GetMouseButtonUp(button);

    /// <summary>
    /// holdKeys가 모두 눌린 상태에서 triggerKey가 눌리는 순간 action을 실행합니다.
    /// </summary>
    public void RegisterSystemKeyCombo(List<Key> holdKeys, Key triggerKey, Action action) => _input.RegisterSystemKeyCombo(holdKeys,triggerKey,action);
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