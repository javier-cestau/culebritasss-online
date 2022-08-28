using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct LocalPlayerData : IComponentData
{
    public FixedString64Bytes playerName;
}
