using Unity.Entities;

[GenerateAuthoringComponent]
public struct SatPath : IComponentData
{
    public int Hops;
    public float Length;
}