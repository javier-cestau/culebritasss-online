using Mix.Data_Components;
using Unity.Entities;
using Unity.NetCode; 
using Unity.Mathematics;
using Unity.Transforms;

namespace Mix.Systems
{
    [UpdateInGroup(typeof(GhostPredictionSystemGroup))]
    public partial class MoveSnakeHeadSystem : SystemBase
    {
        private GhostPredictionSystemGroup m_GhostPredictionSystemGroup;

        protected override void OnCreate()
        {
            m_GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
            RequireSingletonForUpdate<GameConfiguration>();
        }

        protected override void OnUpdate()
        {
            var gameConfiguration = GetSingleton<GameConfiguration>();
            var tick = m_GhostPredictionSystemGroup.PredictingTick;


            Entities
                .WithNone<DeadTag>()
                .ForEach((
                    DynamicBuffer<SnakeInput> inputBuffer, ref Translation trans, ref Node snakeBodyNode,  ref DynamicBuffer<MovementQueue> movementQueue,
                    in SnakeHead snakeHead, in PredictedGhostComponent prediction) =>
                {
                    if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                    {
                        UnityEngine.Debug.Log("No Predicting");
                        return;
                    }
                    if (gameConfiguration.cooldownMovement > 0)
                        return;
                    
                    SnakeInput input; 
                    inputBuffer.GetDataAtTick(tick, out input); 
                    float3 previousPosition = snakeBodyNode.previousPosition;
                    float3 newPosition = trans.Value;
                    float3 previousInputmovementDirection = trans.Value - previousPosition;
                    
                    snakeBodyNode.previousPosition = trans.Value;
                    if (movementQueue.Length == 0 )
                    {
                        trans.Value += previousInputmovementDirection;
                        return;
                    } 
                    
                    newPosition += movementQueue[0].Value;
                    if (movementQueue.Length == 2)
                    {
                        movementQueue[0] = movementQueue[1];
                        movementQueue.RemoveAt(1);
                    }
                    else
                    {
                        movementQueue.RemoveAt(0);
                    }
                    trans.Value = newPosition;

                }).Schedule();
        }
    }
}