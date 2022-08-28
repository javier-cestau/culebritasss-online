using System.Diagnostics;
using Mix.Data_Components;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class FoodSpawningSystem : SystemBase
{
    private Entity _foodPrefab;
    private BeginSimulationEntityCommandBufferSystem _beginSimECB;
    private EntityQuery _foodEntityQuery;
    private int _xBoundary = 8;
    private int _yBoundary = 4;

    protected override void OnCreate()
    {
        //This will grab the BeginSimulationEntityCommandBuffer system to be used in OnUpdate
        _beginSimECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        _foodEntityQuery = GetEntityQuery(ComponentType.ReadOnly<FoodTag>());
        RequireSingletonForUpdate<GameConfiguration>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<SnakeHead>()));
    }

    protected override void OnUpdate()
    {
        var commandBuffer = _beginSimECB.CreateCommandBuffer();

        if (_foodPrefab == Entity.Null)
        {
            _foodPrefab = GetSingleton<FoodSpawner>().prefab;
            return;
        }
        if (_foodEntityQuery.CalculateEntityCountWithoutFiltering() >= 1)
            return;

        var foodPrefab = _foodPrefab;
        var xBoundary = _xBoundary;
        var yBoundary = _yBoundary;
        Job
       .WithCode(() =>
       {
           var foodEntity = commandBuffer.Instantiate(foodPrefab);
           float3 foodPosition = new float3(
                (int)UnityEngine.Random.Range(-xBoundary, xBoundary),
                (int)UnityEngine.Random.Range(-yBoundary, yBoundary),
                0
            );
           commandBuffer.SetComponent<Translation>(foodEntity, new Translation { Value = foodPosition });
       }).Run();
    }
}