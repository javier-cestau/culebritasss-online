using Unity.Entities;

[GenerateAuthoringComponent]
public struct SnakeSpawner : IComponentData
{
    public Entity SnakeHead;
    public Entity SnakeBody;
}