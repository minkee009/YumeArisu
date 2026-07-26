using YumeArisu.Core.Internal.YieldHandling;

namespace YumeArisu.Core.Routines;

public sealed class WaitUntil : YieldInstruction
{
    public Func<bool> Condition { get; private set; } 

    public WaitUntil(Func<bool> condition)
    {
        Condition = condition;
    }
}