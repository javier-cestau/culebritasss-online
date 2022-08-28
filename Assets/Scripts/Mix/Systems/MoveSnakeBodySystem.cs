using Mix.Data_Components;
using Unity.Jobs;
using Unity.NetCode; 
using Unity.Entities;
using Unity.Transforms;

namespace Mix.Systems
{
    [UpdateAfter(typeof(MoveSnakeHeadSystem))]
    [UpdateInGroup(typeof(GhostPredictionSystemGroup))]
    public partial class MoveSnakeBodySystem : SystemBase
    {
        private GhostPredictionSystemGroup m_GhostPredictionSystemGroup;
        private EntityQuery _snakeHeadEntityQuery;

        protected override void OnCreate()
        {
            _snakeHeadEntityQuery = GetEntityQuery(ComponentType.ReadOnly<SnakeHead>());
            m_GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
            RequireSingletonForUpdate<GameConfiguration>();
        }

        protected override void OnUpdate()
        {
            var gameConfiguration = GetSingleton<GameConfiguration>();
            if (gameConfiguration.cooldownMovement > 0)
                return;

            var tick = m_GhostPredictionSystemGroup.PredictingTick;

            JobHandle assignPositionsJob = Entities
                .WithNone<DeadTag>()
                .WithAll<SnakeBodyTag>()
                .ForEach((
                    Entity e,
                    ref Node snakeBodyNode,
                    in Translation trans,
                    in PredictedGhostComponent prediction
                ) =>
                {
                    if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                        return;
                    snakeBodyNode.previousPosition = trans.Value;
                }).Schedule(Dependency);

            var nodes = GetComponentDataFromEntity<Node>();
            Dependency = Entities
                .WithNone<DeadTag>()
                .WithReadOnly(nodes)
                .WithAll<SnakeBodyTag>()
                .ForEach((Entity entity, ref Translation trans, in PredictedGhostComponent prediction) =>
                {
                    if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                        return;
                    var snakeBodyNode = nodes[entity];
                    var nextSnakeBodyNode = nodes[snakeBodyNode.nextSnakeBody];
                    trans.Value = nextSnakeBodyNode.previousPosition;
                }).ScheduleParallel(assignPositionsJob);
        }
    }
}