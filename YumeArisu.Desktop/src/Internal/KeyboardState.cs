using Silk.NET.Input;

namespace YumeArisu.Desktop.Internal;

internal class KeyboardState
{
    public readonly ulong[] Pressed = new ulong[WordCount];
    public readonly ulong[] Released = new ulong[WordCount];
    public readonly ulong[] Current = new ulong[WordCount];

    private const int WordCount = 8;
    private const int KeyCount = WordCount * 64;
    private IKeyboard _keyboard;
    
    public KeyboardState(IKeyboard keyboard)
    {
        _keyboard = keyboard;
        Subscribe(_keyboard);
        Reset();
    }

    public void EndFrame()
    {
        Array.Clear(Pressed, 0, WordCount);
        Array.Clear(Released, 0, WordCount);
    }

    public void ConnectionChanged(IKeyboard keyboard)
    {
        Unsubscribe(_keyboard);
        _keyboard = keyboard;
        Subscribe(_keyboard);
        Reset();
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        int k = (int)key;
        if ((uint)k >= KeyCount) return;
        Pressed[k >> 6] |= 1UL << (k & 63);
        Current[k >> 6] |= 1UL << (k & 63);
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
    {
        int k = (int)key;
        if ((uint)k >= KeyCount) return;
        Released[k >> 6] |= 1UL << (k & 63);
        Current[k >> 6] &= ~(1UL << (k & 63));
    }

    internal void Reset()
    {
        Array.Clear(Pressed, 0, WordCount);
        Array.Clear(Released, 0, WordCount);
        Array.Clear(Current, 0, WordCount);
    }

    public void Subscribe(IKeyboard keyboard)
    {
        keyboard.KeyDown += OnKeyDown;
        keyboard.KeyUp += OnKeyUp;
    }

    public void Unsubscribe(IKeyboard keyboard)
    {
        keyboard.KeyDown -= OnKeyDown;
        keyboard.KeyUp -= OnKeyUp;
    }
}