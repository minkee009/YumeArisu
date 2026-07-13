using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;
using Silk.NET.Input;

public class TestDestroyer : ScriptBehaviour
{
    public override void Update()
    {
        if(Input.GetKeyDown(Key.F) && GameObject != null && !GameObject.IsDestroyed)
        {
            GameObject?.Scene?.DestroyGameObject(GameObject);
        }
    }
}