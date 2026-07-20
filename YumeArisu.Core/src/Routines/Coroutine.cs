using YumeArisu.Core.Internal.YieldAbstraction;

using System.Collections;

namespace YumeArisu.Core.Routines;

public sealed class Coroutine : YieldInstruction
{
    public ScriptBehaviour Owner { get; private set; }  //execution order sorting 처리 목적을 가지고 있음 - 지금 당장 적용시키진 않을 거임
    public IEnumerator Routine { get; private set; }
    public YieldInstruction WaitOption { get; private set; } = null;
    public bool Done 
    { 
        get
        {
            return _done;
        }
        private set
        {
            if (_done == value) 
                return;

            _done = value;
            
            if (_done && _waiter != null)
            {
                //Coroutine.Instance.Reschedule(_waiter);
                _waiter = null;
            }
        }
    }
    
    private Coroutine _waiter = null;
    private bool _done = false;

    public Coroutine(ScriptBehaviour owner, IEnumerator routine)
    {
        Owner = owner;
        Routine = routine;
        Done = false;
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

    internal void SetWaiter(Coroutine waiter) => _waiter = waiter;
}