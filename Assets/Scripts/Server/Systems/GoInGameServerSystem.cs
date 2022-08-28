using System;
using Mix.Data_Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

// When server receives go in game request, go in game and delete request
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class GoInGameServerSystem : SystemBase
{
    private EntityQuery _waypointsQuery;
    private Entity _clientConnectedEntity;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<SnakeSpawner>();
        _waypointsQuery = GetEntityQuery(ComponentType.ReadOnly<Waypoint>(), ComponentType.ReadOnly<Translation>());
    }

    protected override void OnStartRunning()
    {
        _clientConnectedEntity = GetSingletonEntity<ClientsConnected>();
    }

    protected override void OnUpdate()
    {
        if (GetBuffer<ClientsConnected>(_clientConnectedEntity).Length < _waypointsQuery.CalculateEntityCount())
            return;
        
        var snakeHeadprefab = GetSingleton<SnakeSpawner>().SnakeHead;
        var snakeBodyprefab = GetSingleton<SnakeSpawner>().SnakeBody;
        var networkIdFromEntity = GetComponentDataFromEntity<NetworkIdComponent>(true);
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var waypointsTranslations = _waypointsQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        var waypoints = _waypointsQuery.ToComponentDataArray<Waypoint>(Allocator.TempJob);
        int amountOfBodyParts = 3; 
        
        Entities 
            .WithDisposeOnCompletion(waypointsTranslations)
            .WithDisposeOnCompletion(waypoints) 
            .WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity reqEnt, DynamicBuffer<ClientsConnected> clientsConnected) =>
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                UnityEngine.Debug.Log(String.Format("Server setting connection {0} to in game", networkIdFromEntity[clientsConnected[i].SourceConnection].Value));
                commandBuffer.AddComponent<NetworkStreamInGame>(clientsConnected[i].SourceConnection);

                Translation waypointTranslation = waypointsTranslations[i];
                var player = commandBuffer.Instantiate(snakeHeadprefab);
                commandBuffer.SetComponent(player, new GhostOwnerComponent { NetworkId = networkIdFromEntity[clientsConnected[i].SourceConnection].Value });
                commandBuffer.SetComponent(player, waypointTranslation); 
                commandBuffer.SetName(player, "Snake Head");

                float3 snakeHeadStartingMovement = new float3(0, 0, 0);

                switch (waypoints[i].startingMovement)
                {
                    case SpawnDirection.North:
                        snakeHeadStartingMovement = new float3(0, 1, 0);
                        break;

                    case SpawnDirection.South:
                        snakeHeadStartingMovement = new float3(0, -1, 0);
                        break;

                    case SpawnDirection.East:
                        snakeHeadStartingMovement = new float3(1, 0, 0);
                        break;

                    case SpawnDirection.West:
                        snakeHeadStartingMovement = new float3(-1, 0, 0);
                        break;

                    default:
                        break;
                }
                commandBuffer.SetComponent(player, new SnakeHead { startingMovement = snakeHeadStartingMovement });

                commandBuffer.AddBuffer<SnakeInput>(player);
                commandBuffer.SetComponent(clientsConnected[i].SourceConnection, new CommandTargetComponent { targetEntity = player });

                //==== CREATING SNAKE BODIES
                Entity nextSnakeBodyEntity = player;
                float3 snakeBodyDirectionCreation = new float3(0, 0, 0);
                NativeArray<Entity> snakeParts = new NativeArray<Entity>(amountOfBodyParts + 1, Allocator.Temp);

                snakeParts[0] = player;
                for (int j = 0; j < amountOfBodyParts; j++)
                {
                    var bodyEntity = commandBuffer.Instantiate(snakeBodyprefab);

                    switch (waypoints[i].startingMovement)
                    {
                        case SpawnDirection.North:
                            snakeBodyDirectionCreation = new float3(waypointTranslation.Value.x, waypointTranslation.Value.y - 1 - j, 0);
                            break;

                        case SpawnDirection.South:
                            snakeBodyDirectionCreation = new float3(waypointTranslation.Value.x, waypointTranslation.Value.y + 1 + j, 0);
                            break;

                        case SpawnDirection.East:
                            snakeBodyDirectionCreation = new float3(waypointTranslation.Value.x - 1 - j, waypointTranslation.Value.y, 0);
                            break;

                        case SpawnDirection.West:
                            snakeBodyDirectionCreation = new float3(waypointTranslation.Value.x + 1 + j, waypointTranslation.Value.y, 0);
                            break;

                        default:
                            break;
                    }

                    commandBuffer.SetComponent(bodyEntity, new Translation
                    {
                        Value = snakeBodyDirectionCreation
                    });
                    commandBuffer.SetName(bodyEntity, $"Snake Body {j + 1}");
                    snakeParts[j + 1] = bodyEntity;
                }

                float3 snakeBodyPreviousPosition = new float3(0, 0, 0);

                //==== LINKING SNAKE BODIES
                for (int j = 0; j < snakeParts.Length; j++)
                {
                    switch (waypoints[i].startingMovement)
                    {
                        case SpawnDirection.North:
                            snakeBodyPreviousPosition = new float3(waypointTranslation.Value.x, waypointTranslation.Value.y - 1 - j, 0);
                            break;

                        case SpawnDirection.South:
                            snakeBodyPreviousPosition = new float3(waypointTranslation.Value.x, waypointTranslation.Value.y + 1 + j, 0);
                            break;

                        case SpawnDirection.East:
                            snakeBodyPreviousPosition = new float3(waypointTranslation.Value.x - 1 - j, waypointTranslation.Value.y, 0);
                            break;

                        case SpawnDirection.West:
                            snakeBodyPreviousPosition = new float3(waypointTranslation.Value.x + 1 + j, waypointTranslation.Value.y, 0);
                            break;

                        default:
                            break;
                    }
                    var previousSnakeBody = (j  + 1) == snakeParts.Length ? Entity.Null : snakeParts[j + 1];
                    var nextSnakeBody = j == 0 ? Entity.Null : snakeParts[j - 1];
                    commandBuffer.SetComponent(snakeParts[j], new Node
                    {
                        previousSnakeBody = previousSnakeBody,
                        nextSnakeBody = nextSnakeBody,
                        previousPosition = snakeBodyPreviousPosition
                    });
                }

                snakeParts.Dispose();
            }

            
            commandBuffer.SetBuffer<ClientsConnected>(reqEnt);
        }).Run();
        commandBuffer.Playback(EntityManager);
    }
}