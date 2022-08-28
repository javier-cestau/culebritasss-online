using Mix.Data_Components;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

// It is in charge of classitying who's dead
namespace Mix.Systems
{
    [UpdateInGroup(typeof(GhostPredictionSystemGroup))]
    public partial class DeathClassificationSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
        private GhostPredictionSystemGroup m_GhostPredictionSystemGroup;

        protected override void OnCreate()
        {
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            m_GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
            RequireSingletonForUpdate<GameConfiguration>();
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<SnakeHead>()));
        }

        protected override void OnUpdate()
        {
            var gameConfiguration = GetSingleton<GameConfiguration>();
            if (gameConfiguration.cooldownMovement > 0)
                return;
            var commandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
            var tick = m_GhostPredictionSystemGroup.PredictingTick;
            var allTranslations = GetComponentDataFromEntity<Translation>(true);
            var snakeBodyEntities = GetEntityQuery(ComponentType.ReadOnly<SnakeBodyTag>()).ToEntityArray(Allocator.TempJob);

            Entities
                .WithDisposeOnCompletion(snakeBodyEntities)
                .WithReadOnly(allTranslations)
                .WithAll<SnakeHead>()
                .ForEach((Entity entity, in PredictedGhostComponent prediction) =>
                {
                    if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                        return;
                    var translation = allTranslations[entity];

                    if (
                        translation.Value.x >= 9 ||
                        translation.Value.x <= -9 ||
                        translation.Value.y >= 5 ||
                        translation.Value.y <= -5
                    )
                    {
                        commandBuffer.AddComponent(entity, new DeadTag());
                    }
                    foreach (var snakeBodyEntity in snakeBodyEntities)
                    {
                        if (translation.Value.Equals(allTranslations[snakeBodyEntity].Value))
                        {
                            commandBuffer.AddComponent(entity, new DeadTag());
                            break;
                        }
                    }
                }).Schedule();

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}