using Mix.Data_Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine; 

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public partial class SnakeInputSystem : SystemBase
{
    private ClientSimulationSystemGroup _mClientSimulationSystemGroup;
    private float3 _lastDirection;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        _mClientSimulationSystemGroup = World.GetExistingSystem<ClientSimulationSystemGroup>();
    }

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            var commandTargetEntity = GetSingletonEntity<CommandTargetComponent>();
            float3 tempLastDirection = new float3(0, 0, 0);
            Entities.WithNone<SnakeInput>().ForEach(
                (Entity ent, in GhostOwnerComponent ghostOwner, in SnakeHead snakeHead) =>
            {
                if (ghostOwner.NetworkId == localPlayerId)
                {
                    tempLastDirection.x = snakeHead.startingMovement.x;
                    tempLastDirection.y = snakeHead.startingMovement.y;
                    commandBuffer.AddBuffer<SnakeInput>(ent);
                    commandBuffer.SetComponent(commandTargetEntity, new CommandTargetComponent { targetEntity = ent });
                }
            }).Run();
            commandBuffer.Playback(EntityManager);
            _lastDirection = tempLastDirection;
            return;
        }
        var input = default(SnakeInput);
        input.Tick = _mClientSimulationSystemGroup.ServerTick;
        input.movementDirection = _lastDirection;
        if (Input.GetKey("a"))
            input.movementDirection = new float3(-1, 0, 0 );
        if (Input.GetKey("d"))
            input.movementDirection = new float3(1, 0, 0 );
        if (Input.GetKey("s"))
            input.movementDirection = new float3(0, -1, 0 );
        if (Input.GetKey("w"))
            input.movementDirection = new float3(0, 1, 0 );
        _lastDirection = input.movementDirection;
        var inputBuffer = EntityManager.GetBuffer<SnakeInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}