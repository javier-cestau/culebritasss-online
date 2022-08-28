using Mix.Data_Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInWorld(TargetWorld.Server)]
[UpdateInGroup(typeof(GhostPredictionSystemGroup), OrderLast = true)]
public partial class CooldownSystem : SystemBase
{
    private GhostPredictionSystemGroup m_GhostPredictionSystemGroup;

    protected override void OnCreate()
    {
        m_GhostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
        RequireSingletonForUpdate<GameConfiguration>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<SnakeHead>()));
    }

    protected override void OnUpdate()
    {
        var deltaTime = m_GhostPredictionSystemGroup.Time.DeltaTime;
        Entities.ForEach((ref GameConfiguration gameConfiguration) =>
        {
            if (gameConfiguration.cooldownMovement > 0)
            {
                gameConfiguration.cooldownMovement -= deltaTime;
                return;
            }
            gameConfiguration.cooldownMovement = .3f;
        }).Schedule();
    }
}