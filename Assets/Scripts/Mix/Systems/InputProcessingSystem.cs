using Mix.Data_Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public partial class InputProcessingSystem : SystemBase
{
    private GhostPredictionSystemGroup m_GhostPredictionSystemGroup;

    protected override void OnCreate()
    {           
        m_GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
        RequireSingletonForUpdate<GameConfiguration>();
    }
    
    protected override void OnUpdate()
    {
        var tick = m_GhostPredictionSystemGroup.PredictingTick;
        Entities
            .WithNone<DeadTag>()
            .ForEach((
                DynamicBuffer<SnakeInput> inputBuffer, ref DynamicBuffer<MovementQueue> movementQueue, 
                in Translation trans, in Node snakeBodyNode, in PredictedGhostComponent prediction) =>
            {
                if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                    return;
                inputBuffer.GetDataAtTick(tick, out SnakeInput input);
                if (input.movementDirection.Equals(float3.zero))
                    return;
                if (movementQueue.Length == 2)
                    return;
        
                float3 previousInputMovementDirection = movementQueue.Length == 0 ? 
                    (trans.Value - snakeBodyNode.previousPosition) : 
                    movementQueue[0].Value;
                
                float3 differenceInputs = input.movementDirection + previousInputMovementDirection;
                if (!float3.zero.Equals(differenceInputs) && math.abs(differenceInputs.x) != 2 && math.abs(differenceInputs.y) != 2 )
                {
                    movementQueue.Add(new MovementQueue() { Value = input.movementDirection });
                }
            }).Schedule();
    }
}
