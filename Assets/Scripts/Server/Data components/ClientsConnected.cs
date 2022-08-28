using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ClientsConnected : IBufferElementData
{
    public FixedString64Bytes playerName;
    public Entity SourceConnection;
}
