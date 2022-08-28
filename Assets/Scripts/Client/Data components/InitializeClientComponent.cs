using Unity.Collections;
using Unity.Entities;
namespace Client.Data_components
{
    [GenerateAuthoringComponent]
    public struct InitializeClientComponent : IComponentData
    {
        public FixedString64Bytes playerName;

    }
}