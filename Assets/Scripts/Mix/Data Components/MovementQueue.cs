using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Mix.Data_Components
{
    [GenerateAuthoringComponent]
    [InternalBufferCapacity(2)]
    public struct MovementQueue : IBufferElementData
    {
        [GhostField]
        public float3 Value;
    }
}
