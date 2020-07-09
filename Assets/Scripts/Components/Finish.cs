using Unity.Entities;

[GenerateAuthoringComponent]
public struct Finish : IComponentData
{
    public Entity Value;
}
