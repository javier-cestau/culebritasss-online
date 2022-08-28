using System;
using Unity.Entities;
using Unity.NetCode;

// Create a custom bootstrap which enables auto connect.
// The bootstrap can also be used to configure other settings as well as to
// manually decide which worlds (client and server) to create based on user input
[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    // public override bool Initialize(string defaultWorldName)
    // {
    //     AutoConnectPort = 7979; // Enabled auto connect
    //     return base.Initialize(defaultWorldName); // Use the regular bootstrap
    // }
    
    public override bool Initialize(string defaultWorldName)
    {

        var world = new World(defaultWorldName);
        World.DefaultGameObjectInjectionWorld = world;
        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        GenerateSystemLists(systems);

        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, ExplicitDefaultWorldSystems);
#if !UNITY_DOTSRUNTIME
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
#endif
#if  UNITY_SERVER
        CreateServerWorld(world, "ServerWorld");

#endif
        return true;
    }
}