using System.Collections;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Internal.YieldHandling;

namespace YumeArisu.Core.Routines;

public sealed class Coroutine : YieldInstruction
{
    public ScriptBehaviour Owner { get; }
    public IEnumerator Routine { get; }
    public YieldInstruction WaitOption { get; private set; }
    public bool Done { get; private set; }

    internal WaitListState ListState { get; set; }
    internal LinkedListNode<Coroutine> Node { get; set; }

    private List<Coroutine> _waiters; // 나를 기다리고 있는 코루틴 리스트 <- 사실상 소유권을 취득한 것임 == 막 휘둘러도 됨

    public Coroutine(ScriptBehaviour owner, IEnumerator routine)
    {
        Owner = owner;
        Routine = routine;
    }

    internal bool MoveNext()
    {
        if (Done)
            return false;

        bool result = Routine.MoveNext();

        if (Done)
        {
            // Routine.MoveNext() 실행 도중 ForceStop()이 재진입 호출되어
            // 이미 종료 처리됨 (예: 코루틴이 자기 자신의 오너를 Destroy한 경우).
            // 여기서 상태를 덮어쓰면 안 됨 - 웨이커 처리는 ForceStop이 이미 수행함.
            return false;
        }

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