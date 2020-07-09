using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class OrbitSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.
        WithAll<SatTag>().
        ForEach((ref Translation translation, ref Rotation rotation, in LocalToWorld localToWorld, in Speed speed) =>
        {
            translation.Value = RotateAroundAxis(translation.Value, -math.normalizesafe(localToWorld.Up), math.radians(0.07f * speed.Value) * deltaTime);
            rotation.Value = math.mul(rotation.Value, quaternion.RotateY(math.radians(-0.07f * speed.Value * deltaTime)));

        }).ScheduleParallel();
}

    public static float3 RotateAroundAxis(float3 position, float3 axis, float delta) => math.mul(quaternion.AxisAngle(axis, delta), position);
}
