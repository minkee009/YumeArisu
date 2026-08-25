using System.Collections;
using System.Numerics;
using Silk.NET.Input;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Routines;
using YumeArisu.Core.Systems;

namespace YumeArisu.Game.Scripts;

public class CamMover : ScriptBehaviour
{
    public override void OnUpdate()
    {
        float xInput = (Input.GetKey(Key.D) ? 1 : 0) + (Input.GetKey(Key.A) ? -1 : 0);
        float zInput = (Input.GetKey(Key.S) ? 1 : 0) + (Input.GetKey(Key.W) ? -1 : 0);
        float yInput = (Input.GetKey(Key.E) ? 1 : 0) + (Input.GetKey(Key.Q) ? -1 : 0);

        Transform.LocalPosition += new Vector3(xInput, yInput, zInput) * 5.0f * Time.DeltaTime; 
    }
}