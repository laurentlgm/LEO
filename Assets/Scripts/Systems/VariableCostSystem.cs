using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(NeighborSystem))]

public class VariableCostSystem : SystemBase
{

    protected override void OnUpdate()
    {
        Entities.
        WithAll<SatTag>().
        ForEach((ref DynamicBuffer<Neighbors> neighbors, ref DistToStart distToStart, in Translation translation) =>
        {
            ComponentDataFromEntity<Translation> allTranslations = GetComponentDataFromEntity<Translation>(true);

            Neighbors East = neighbors[3];
            Neighbors West = neighbors[4];

            East.Cost = math.distance(translation.Value, allTranslations[East.Node].Value);
            // Making East and West costs equal did not work for routig...
            West.Cost = math.distance(translation.Value, allTranslations[West.Node].Value);

            neighbors[3] = East;
            neighbors[4] = West;
        }).ScheduleParallel();
    }
}