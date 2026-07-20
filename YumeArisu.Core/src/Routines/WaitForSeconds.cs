using YumeArisu.Core.Internal.YieldAbstraction;

namespace YumeArisu.Core.Routines;

public class WaitForSeconds : YieldInstruction
{
    public float Seconds { get; private set; }

    public WaitForSeconds(float seconds)
    {
        Seconds = seconds;
    }
}