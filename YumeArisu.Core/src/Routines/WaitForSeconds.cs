using YumeArisu.Core.Internal.YieldHandling;

namespace YumeArisu.Core.Routines;

public sealed class WaitForSeconds : YieldInstruction
{
    public float Seconds { get; private set; }

    public WaitForSeconds(float seconds)
    {
        Seconds = seconds;
    }
}