using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class AirportMoveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.
        WithAll<AirportTag>().
        ForEach((ref Translation translation, in Speed speed) =>
        {
            translation.Value = RotateAroundAxis(translation.Value, new float3(0,1,0), math.radians(-0.004f * speed.Value) * deltaTime);
        }).ScheduleParallel();
    }

    public static float3 RotateAroundAxis(float3 position, float3 axis, float delta) => math.mul(quaternion.AxisAngle(axis, delta), position);
}
