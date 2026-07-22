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

    private List<Coroutine> _waiters; // 나를 기다리고 있는 코루틴 리스트 <- 사실상 소유권을 취득한 것임 == 막 휘둘러도 됨

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

        if (Done && _waiters != null && _waiters.Count > 0)
        {
            var toWake = _waiters;
            _waiters = null;
            foreach (var waiter in toWake)
                CoroutineSystem.Instance.Proccess(waiter); // 끝나자마자 바로 깨움 
        }

        return result;
    }

    internal void ForceStop()
    {
        if (Done)
            return;

        Done = true;

        // 다 깨워잇
        if (_waiters != null && _waiters.Count > 0)
        {
            var toWake = _waiters;
            _waiters = null;
            foreach (var waiter in toWake)
                CoroutineSystem.Instance.Proccess(waiter);
        }
    }

    internal void AddWaiter(Coroutine waiter)
    {
        _waiters ??= new List<Coroutine>();
        _waiters.Add(waiter);
    }

    internal void RemoveWaiter(Coroutine waiter) => _waiters?.Remove(waiter);
}