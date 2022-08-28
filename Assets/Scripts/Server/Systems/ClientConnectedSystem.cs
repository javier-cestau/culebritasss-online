using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEditor.PackageManager;

public partial class ClientConnectedSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBuffer;
    protected override void OnCreate()
    {
        _endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<IncomingClientConnection>(), ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()));
    }
    protected override void OnUpdate()
    {
        var commandBuffer = _endSimulationEntityCommandBuffer.CreateCommandBuffer();
        var clientsConnectedEntity = GetSingletonEntity<ClientsConnected>();
 
        Entities  
            .WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity reqEnt, in IncomingClientConnection req, in ReceiveRpcCommandRequestComponent reqSrc) =>
            {
                var clientsConnectedBuffer = GetBuffer<ClientsConnected>(clientsConnectedEntity);
                clientsConnectedBuffer.Add(new ClientsConnected() { playerName = req.playerName, SourceConnection = reqSrc.SourceConnection });
                var clientConnectedEntity = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(clientConnectedEntity, new IncomingClientConnection());
                commandBuffer.DestroyEntity(reqEnt);
            }).Schedule();
    }
}
