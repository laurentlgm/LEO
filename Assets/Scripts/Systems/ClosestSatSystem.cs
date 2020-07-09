using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(NeighborSystem))]

public class ClosestSatSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.
        WithAll<SatTag>().
        ForEach((ref DistToStart distStart, ref DistToFinish distFinish, ref CloseGroup closeGroup, in Translation translation, in Start start, in Finish finish, in SlantDistance slantDistance) =>
        {
            if (start.Value != Entity.Null)
            {
                ComponentDataFromEntity<Translation> allTranslations = GetComponentDataFromEntity<Translation>(true);
                distStart.Value = math.distance(translation.Value, allTranslations[start.Value].Value);
                distFinish.Value = math.distance(translation.Value, allTranslations[finish.Value].Value);
                if (distStart.Value <= slantDistance.Value)
                {
                    closeGroup.Value = 1;
                }
                else if (distFinish.Value <= slantDistance.Value)
                {
                    closeGroup.Value = 2;
                }
                else
                {
                    closeGroup.Value = 0;
                }
            }
        }).ScheduleParallel();
    }
}