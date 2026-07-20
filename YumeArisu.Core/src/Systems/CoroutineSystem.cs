using YumeArisu.Core.Routines;
using YumeArisu.Core.Internal.YieldAbstraction;

namespace YumeArisu.Core.Systems;

public class CoroutineSystem : SystemBase<TimeSystem, NoConfig>
{
    private LinkedList<Coroutine> _unknownReturns;
    private LinkedList<Coroutine> _coroutineReturns;

    // FixedUpdate
    // Update
    // AfterRender
    // WaitSeconds
    // WaitUntil

    internal override void ShutDownInternal()
    {
        throw new NotImplementedException();
    }

    internal override void StartUpInternal(NoConfig config)
    {
        throw new NotImplementedException();
    }

    public void ImmediateStopAllCoroutines()
    {
        // TODO : 씬 전환을 대상으로 하기 때문에 전체 코루틴을 Stop 및 Discard해야 함.
    }
}