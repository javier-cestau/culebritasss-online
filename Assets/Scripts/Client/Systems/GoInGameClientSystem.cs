using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

// When client has a connection with network id, go in game and tell server to also go in game
 
    [UpdateInGroup(typeof(ClientSimulationSystemGroup))]
    public partial class GoInGameClientSystem : SystemBase
    {
        protected override void OnCreate()
        {
            // Make sure we wait with the sub scene containing the prefabs to load before going in-game
            RequireSingletonForUpdate<LocalPlayerData>();
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<NetworkIdComponent>(), ComponentType.Exclude<NetworkStreamInGame>()));
        }

        protected override void OnUpdate()
        {
            var localPlayerData = GetSingleton<LocalPlayerData>();

            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithNone<NetworkStreamInGame>().ForEach((Entity ent, in NetworkIdComponent id) =>
            {
                commandBuffer.AddComponent<NetworkStreamInGame>(ent);
                var req = commandBuffer.CreateEntity(); 
                commandBuffer.AddComponent(req, new IncomingClientConnection() { playerName = localPlayerData.playerName });
                commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
                commandBuffer.SetName(req, "ClientConnection");
            }).Run();
            commandBuffer.DestroyEntity( GetSingletonEntity<LocalPlayerData>());
            commandBuffer.Playback(EntityManager);
        }
    } 