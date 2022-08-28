using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum SpawnDirection { North, East, South, West };
 

[GenerateAuthoringComponent]
public struct Waypoint : IComponentData
{ 
    public SpawnDirection startingMovement;
    
}
