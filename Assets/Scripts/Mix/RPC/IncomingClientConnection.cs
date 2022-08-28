using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
 
public struct IncomingClientConnection : IRpcCommand
{
    public FixedString64Bytes playerName;

} 