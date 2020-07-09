using Unity.Entities;

[InternalBufferCapacity(6)]
public struct Neighbors : IBufferElementData
{
    public Entity Node;
    public float Cost;
}
