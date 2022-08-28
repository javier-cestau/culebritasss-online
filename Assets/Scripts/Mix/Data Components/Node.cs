using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;

namespace Mix.Data_Components
{
    [GenerateAuthoringComponent]
    public struct Node : IComponentData
    {
        [GhostField]
        public Entity previousSnakeBody;

        [GhostField]
        public Entity nextSnakeBody;

        [GhostField]
        public float3 previousPosition;



    }
}
