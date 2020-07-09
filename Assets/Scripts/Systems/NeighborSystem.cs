using Unity.Entities;
using Unity.Jobs;


public class NeighborSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Adds airport as a sat neighbor. Group 1 = start, Group 2 = finish.
        Entities.
        WithAll<SatTag>().
        ForEach((ref DynamicBuffer<Neighbors> neighbors, in CloseGroup closeGroup, in Start start, in Finish finish, in DistToStart distStart, in DistToFinish distFinish) =>
        {
            Neighbors nei;

            if (closeGroup.Value == 1)
            {
                nei.Node = start.Value;
                nei.Cost = distStart.Value;
                neighbors[5] = nei;
            }
            else if (closeGroup.Value == 2)
            {
                nei.Node = finish.Value;
                nei.Cost = distFinish.Value;
                neighbors[5] = nei;
            }
            else
            {
                nei.Node = Entity.Null;
                nei.Cost = 10f;
                neighbors[5] = nei;
            }
        }).ScheduleParallel();
    }

}
