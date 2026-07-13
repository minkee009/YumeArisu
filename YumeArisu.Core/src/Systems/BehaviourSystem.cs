using System.Runtime.CompilerServices;
using YumeArisu.Core.Routines;

namespace YumeArisu.Core.Systems;

public class BehaviourSystem : SystemBase<BehaviourSystem, NoConfig>
{    private List<Behaviour> _behaviours;

    //foreach 순회도중 리스트가 사라지지 않게 behaviour 등록/해제 버퍼
    private Queue<Action> _commandQueue;

    internal override void StartUpInternal(NoConfig control)
    {
        _behaviours = new();
        _commandQueue = new();
    }

    internal override void ShutDownInternal()
    {
        _behaviours.Clear();
        _commandQueue.Clear();
        _behaviours = null;
        _commandQueue = null;
    }

    internal void RegisterBehaviour(Behaviour bh)
    {
        _commandQueue.Enqueue(() => _behaviours.Add(bh));
    }

    internal void UnregisterBehaviour(Behaviour bh)
    {
        _commandQueue.Enqueue(() => _behaviours.Remove(bh));
    }

    public void BeginFrame()
    {
        while(_commandQueue.Count > 0)
        {
            _commandQueue.Dequeue()();
        }  
    }

    public void ExecuteUpdate()
    {
        foreach(var bh in _behaviours)
        {
            if(!bh.GameObject.IsDestroyed)
                bh.Update();
        }
    }

    public void ExecuteFixedUpdate()
    {
        foreach(var bh in _behaviours)
        {
            if(!bh.GameObject.IsDestroyed)
                bh.FixedUpdate();
        }
    }
}