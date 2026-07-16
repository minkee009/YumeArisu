using System.Collections;

namespace YumeArisu.Core.Routines;

public sealed class Coroutine
{
    public IEnumerator Routine { get; private set; }
    public string Name { get; private set; }
    public object Owner { get; private set; }
    
    // public Coroutine(IEnumerator routine, string name, object owner)
    // {
    //     Routine = routine;
    //     Name = name;
    //     Owner = owner;
    // }
}