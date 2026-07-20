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

    internal LinkedListNode<Coroutine> SchedulerNode { get; set; }

    private Coroutine _waiter; // 나를 기다리고 있는 코루틴 (없으면 null)

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

        if (Done && _waiter != null)
        {
            var waiter = _waiter;
            _waiter = null;
            CoroutineSystem.Instance.Proccess(waiter); // 끝나자마자 바로 깨움
        }

        return result;
    }

    internal void ForceStop()
    {
        Done = true;
        _waiter = null;
    }

    internal void SetWaiter(Coroutine waiter) => _waiter = waiter;
    internal void ClearWaiter(Coroutine waiter)
    {
        if (_waiter == waiter)
            _waiter = null;
    }
}