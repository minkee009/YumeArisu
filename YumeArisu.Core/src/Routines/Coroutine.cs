using System.Collections;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Internal.YieldAbstraction;

namespace YumeArisu.Core.Routines;

public sealed class Coroutine : YieldInstruction
{
    public ScriptBehaviour Owner { get; private set; }  //execution order sorting 처리 목적을 가지고 있음 - 지금 당장 적용시키진 않을 거임
    public IEnumerator Routine { get; private set; }
    public YieldInstruction WaitOption { get; private set; } = null;
    public bool Done 
    { 
        get => _done;
        internal set
        {
            if(_done == value)
                return;

            _done = value;

            if(_done && _waifuList.Count > 0)
            {
                foreach(var waifu in _waifuList)
                    waifu.WakeTheF___UpSamurai();
                
                _waifuList.Clear();
            }
        } 
    }

    internal LinkedListNode<Coroutine> SchedulerNode { get; set; }

    private List<Coroutine> _waifuList = new();
    private bool _done = false;

    public Coroutine(ScriptBehaviour owner, IEnumerator routine)
    {
        Owner = owner;
        Routine = routine;
    }

    internal bool MoveNext()
    {
        var result = Routine.MoveNext();

        if (result)
        {
            var yield = Routine.Current;
            if(yield is YieldInstruction waitOption)
                WaitOption = waitOption;
            else
                WaitOption = null;
        }
        else
        {
            Done = true;
        }

        return result; 
    }

    /// <summary>
    /// 기다리는 코루틴을 참조합니다.
    /// </summary>
    /// <param name="waiter">자신의 루틴이 끝나길 기대하는 코루틴</param>
    internal void SleepingWaifu(Coroutine waifu)
    {
        _waifuList.Add(waifu);
    }

    /// <summary>
    /// 자신이 다른 코루틴을 기다리고 있다면 깨웁니다.
    /// </summary>
    internal void WakeTheF___UpSamurai()
    {
        WaitOption = null;
        CoroutineSystem.Instance.Reschedule(this);
    }
}