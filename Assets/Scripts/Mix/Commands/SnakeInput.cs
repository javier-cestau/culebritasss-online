using Unity.Mathematics;
using Unity.NetCode;

public struct SnakeInput : ICommandData
{
    public uint Tick { get; set; }
    public float3 movementDirection;
}