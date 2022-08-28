using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Mix.Data_Components
{
    [GenerateAuthoringComponent]
    public struct SnakeHead : IComponentData
    {
        [GhostField]
        public float3 startingMovement;

    }
}