using System;
using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct GameConfiguration : IComponentData
{
    [GhostField]
    public float cooldownMovement;
}