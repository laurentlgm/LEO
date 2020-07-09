using Unity.Entities;

[GenerateAuthoringComponent]
public struct Dijkstra : IComponentData
{
    public float SPTCost;
    public Entity PrevNode;
}
