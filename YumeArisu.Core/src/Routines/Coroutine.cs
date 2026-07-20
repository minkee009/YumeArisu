using System.Collections;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Internal.YieldAbstraction;

namespace YumeArisu.Core.Routines;

public sealed class Coroutine : YieldInstruction
{
    public ScriptBehaviour Owner { get; }
    public IEnumerator Routine { get; }
    public YieldInstruction WaitOption { get; private set; }
    public bool Done { get; private set; }

    public Coroutine(ScriptBehaviour owner, IEnumerator routine)
    {
        Owner = owner;
        Routine = routine;
    }

    internal bool MoveNext()
    {
        bool result = Routine.MoveNext();
        WaitOption = result ? Routine.Current as YieldInstruction : null;
        Done = !result;
        return result;
    }
}