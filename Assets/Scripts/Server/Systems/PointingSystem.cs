using Mix.Data_Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class PoitingSystem : SystemBase
{
    private EntityQuery _foodEntityQuery;
    private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
    private Entity _snakeBodyPrefab;

    protected override void OnCreate()
    {
        _foodEntityQuery = GetEntityQuery(ComponentType.ReadOnly<FoodTag>());
        _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        _snakeBodyPrefab = GetSingleton<SnakeSpawner>().SnakeBody;
    }

    protected override void OnUpdate()
    {
        var commandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        var foodNativeArray = _foodEntityQuery.ToEntityArray(Allocator.TempJob);
        var allTranslations = GetComponentDataFromEntity<Translation>(true);
        var allNodes = GetComponentDataFromEntity<Node>();
        var snakeBodyPrefab = _snakeBodyPrefab;
        Entities
            .WithDisposeOnCompletion(foodNativeArray)
            .WithReadOnly(allTranslations)
            .WithAll<SnakeHead>()
            .ForEach((Entity snakeHeadEntity, in Translation translation) =>
        {
            if (foodNativeArray.Length > 0)
            {
                Entity foodEntity = foodNativeArray[0];
                if (
                    allTranslations[foodEntity].Value.x == translation.Value.x &&
                    allTranslations[foodEntity].Value.y == translation.Value.y
                   )
                {
                    var searchForSnakeTail = allNodes[snakeHeadEntity];
                    var tailEntity = Entity.Null;
                    var counter = 0;
                    // Not having a previous body means we have found the tail
                    while (searchForSnakeTail.previousSnakeBody != Entity.Null)
                    {
                        tailEntity = searchForSnakeTail.previousSnakeBody;
                        searchForSnakeTail = allNodes[searchForSnakeTail.previousSnakeBody];
                        counter++;
                    }
                    var snakeNodeCurrentTail = searchForSnakeTail;

                    var newSnakeTail = commandBuffer.Instantiate(snakeBodyPrefab);

                    commandBuffer.SetComponent(newSnakeTail, new Translation { Value = new Unity.Mathematics.float3(100, 0, 0) });
                    commandBuffer.SetComponent(newSnakeTail, new Node { nextSnakeBody = tailEntity });
                    commandBuffer.SetName(newSnakeTail, $"Snake Body {counter + 1}");
                    commandBuffer.SetComponent(tailEntity, new Node
                    {
                        nextSnakeBody = snakeNodeCurrentTail.nextSnakeBody,
                        previousSnakeBody = newSnakeTail,
                        previousPosition = snakeNodeCurrentTail.previousPosition
                    });
                    commandBuffer.DestroyEntity(foodEntity);
                }
            }
        }).Schedule();
        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}