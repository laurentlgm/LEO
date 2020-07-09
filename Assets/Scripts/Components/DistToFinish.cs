using Unity.Entities;

[GenerateAuthoringComponent]
public struct DistToFinish : IComponentData
{
    public float Value;
}