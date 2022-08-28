using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using Unity.NetCode;
using UnityEngine;
using Unity;
using System;
#if !UNITY_SERVER
using Client.Data_components;
#endif
using Server.Data_components;

#if UNITY_EDITOR
using Unity.NetCode.Editor;
#endif
 
#if !UNITY_CLIENT
//ServerConnectionControl is run in ServerWorld and starts listening on a port
//The port is provided by the ServerDataComponent
[UpdateInWorld(TargetWorld.Server)]
public partial class ServerConnectionControl : SystemBase
{
    private ushort m_GamePort = 7979;
    protected override void OnCreate()
    {
        // We require the InitializeServerComponent to be created before OnUpdate runs
        RequireSingletonForUpdate<InitializeServerComponent>();
    }
    protected override void OnUpdate()
    { 

        // This is used to split up the game's "world" into sections ("tiles")
        // The client is in a "tile" and networked objects are in "tiles"
        // the client is streamed data based on tiles that are near them
        //https://docs.unity3d.com/Packages/com.unity.netcode@0.5/manual/ghost-snapshots.html
        //check out "Distance based importance" in the link above
        var grid = EntityManager.CreateEntity();
        EntityManager.AddComponentData(grid, new GhostDistanceImportance
        {
            ScaleImportanceByDistance = GhostDistanceImportance.DefaultScaleFunctionPointer,
            TileSize = new int3(80, 80, 80),
            TileCenter = new int3(0, 0, 0),
            TileBorderWidth = new float3(1f, 1f, 1f)
        });

        //Here is where the server creates a port and listens
        NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
        ep.Port = m_GamePort;
        World.GetExistingSystem<NetworkStreamReceiveSystem>().Listen(ep);
        Debug.Log("Server is listening on port: " + m_GamePort.ToString());
        EntityManager.DestroyEntity(GetSingletonEntity<InitializeServerComponent>());
    }
}
#endif
#if !UNITY_SERVER
//ClientConnectionControl is run in ClientWorld and connects to an IP address and port
//The IP address and port is provided by the ClientDataComponent
[UpdateInWorld(TargetWorld.Client)]

public partial class ClientConnectionControl : SystemBase
{
    protected override void OnCreate()
    {
        // We require the InitializeServerComponent to be created before OnUpdate runs
        RequireSingletonForUpdate<InitializeClientComponent>();
    }
    
    protected override void OnUpdate()
    {  
        NetworkEndPoint ep = NetworkEndPoint.Parse("127.0.0.1", 7979);
        World.GetExistingSystem<NetworkStreamReceiveSystem>().Connect(ep);
        Debug.Log("Client connecting to ip: 127.0.0.1 and port: " + 7979.ToString());
        var initializeClientComponent = GetSingleton<InitializeClientComponent>();
        var playerLocalDataEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(playerLocalDataEntity, new LocalPlayerData() { playerName = initializeClientComponent.playerName});
        EntityManager.DestroyEntity(GetSingletonEntity<InitializeClientComponent>());
    }
}
#endif