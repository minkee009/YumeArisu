using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace YumeArisu.Core.Abstractions;

public interface IInputSource : IDisposable
{
    public abstract bool UpdateMustEndOfFrame { get; }

    public void Update(); // 내부 상태 갱신
    
    public bool GetKey(Key key);
    public bool GetKeyDown(Key key);
    public bool GetKeyUp(Key key);

    public Vector2 GetMousePosition();
    public Vector2 GetMouseDelta();

    public bool GetMouseButton(MouseButton button);
    public bool GetMouseButtonDown(MouseButton button);
    public bool GetMouseButtonUp(MouseButton button);

    public void OnViewResize(Vector2D<int> size);

    public void RegisterSystemKeyCombo(List<Key> holdKeys, Key triggerKey, Action action);
}