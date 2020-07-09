using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;



public class SatScript : MonoBehaviour
{
    public Controller ctr;

    private float3[] satPosition;
    private float3[] satProjection;

    private quaternion[] satRotation;
    private NativeArray<int>[] satNeighbor;
    private int[] satID;

    private void Start()
    {
       CreateSatEntities();
    }

    public void ResetConstellation()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        NativeArray<Entity> sats = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SatTag>()).ToEntityArray(Allocator.TempJob);
        entityManager.DestroyEntity(sats);
        sats.Dispose();

        CreateSatEntities();
    }


    public void CreateSatEntities()
    {
        CreateSatPositions(ctr.planes, ctr.satellites);
        int totalSats = ctr.planes * ctr.satellites;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        Destroy(cube);
        Material material = new Material(Shader.Find("Standard"))
        {
            enableInstancing = true,
            //color = new Color32(255, 117, 0, 255)
            color = Color.white
        };

        float distInOrbit = math.distance(satPosition[0], satPosition[1]);

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype entityArchetype = entityManager.CreateArchetype(

            typeof(SatTag),
            typeof(SatID),
            typeof(SlantDistance),
            typeof(Neighbors),
            typeof(Start),
            typeof(Finish),
            typeof(DistToStart),
            typeof(DistToFinish),
            typeof(CloseGroup),
            typeof(Dijkstra),
            typeof(Translation),
            typeof(Speed),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderBounds),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(totalSats, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            entityManager.SetComponentData(entity, new Scale { Value = 0.008f });
            entityManager.SetSharedComponentData(entity, new RenderMesh { mesh = mesh, material = material });

            entityManager.SetComponentData(entity, new Translation { Value = satPosition[i] });
            entityManager.SetComponentData(entity, new Rotation { Value = satRotation[i] });
            entityManager.SetComponentData(entity, new SatID { Value = satID[i] });
            entityManager.SetComponentData(entity, new Speed { Value = ctr.speedMultiplier });
            entityManager.SetComponentData(entity, new CloseGroup { Value = 0 });

            // Fixed Neighbors + Airport (North and South are constant costs):
            DynamicBuffer<Neighbors> neighbors = entityManager.GetBuffer<Neighbors>(entity);
            neighbors.Add(new Neighbors { Node = entityArray[satNeighbor[i][0]], Cost = 0f }); // distance to itself
            neighbors.Add(new Neighbors { Node = entityArray[satNeighbor[i][1]], Cost = distInOrbit }); // distance South
            neighbors.Add(new Neighbors { Node = entityArray[satNeighbor[i][2]], Cost = distInOrbit }); // distance North
            neighbors.Add(new Neighbors { Node = entityArray[satNeighbor[i][3]], Cost = 10f }); // Infinity to East
            neighbors.Add(new Neighbors { Node = entityArray[satNeighbor[i][4]], Cost = 10f }); // Infinity to West
            neighbors.Add(new Neighbors { Node = Entity.Null, Cost = 10f }); // For the Airport

            // Max distance between observer an satellite:
            entityManager.SetComponentData(entity, new SlantDistance { Value = ctr.slantDistance });

            // Dijkstra specific:
            entityManager.SetComponentData(entity, new Dijkstra { SPTCost = 0f , PrevNode = entity});
        }
        entityArray.Dispose();
    }


    private void CreateSatPositions(int planeCount, int satCount)
    {
        int totalSats = planeCount * satCount;

        satPosition = new float3[totalSats];
        satProjection = new float3[totalSats];
        satRotation = new quaternion[totalSats];
        satNeighbor = new NativeArray<int>[totalSats];
        satID = new int[totalSats]; //Probably not needed but I want to make sure ID does not get mixed.

        float x;
        float y;
        float z;
        float inclRad;  // Between planes and equator.
        float beta;     // Angle between orbital planes.
        // Argument of latitude (u) = angle between the orbit diameter vector and
        // the satellite position in its own orbit:
        float u = 360f / satCount;
        // Delta Theta is the phase difference between adjacent planes.
        // From walker delta formula:
        float dth = ctr.f * (360f / totalSats);
        float orbitRadius = (ctr.earthRadius + ctr.altitude) / ctr.earthRadius; // Orbit radius

        beta = 360f / planeCount;
        inclRad = ctr.inclination * Mathf.PI / 180; //Radians.

        GameObject tempSat = new GameObject();

        // Initial coordianates for the first orbit:
        x = orbitRadius * Mathf.Cos(inclRad);
        y = orbitRadius * Mathf.Sin(inclRad);
        z = 0; // Just to make it easier to calculate the first porition.

        int id = 0;
        for (int p = 0; p < planeCount; p++)
        {
            for (int s = 0; s < satCount; s++)
            {
                //Move sat to its orbit:
                tempSat.transform.position = new float3(x, y, z);
                tempSat.transform.rotation = quaternion.identity;

                //Point the sat to Earth:
                tempSat.transform.Rotate(0, 0, ctr.inclination);

                //Put the plane in the right angle around Earth's vertical axis:
                tempSat.transform.RotateAround(Vector3.zero, Vector3.up, beta * p);

                //Put the sat in the right place in the orbit:
                //There's no true anomaly or mean anomaly on a circular orbit so
                //this is just the argument of latitude (u) plus the delta phase:
                tempSat.transform.RotateAround(Vector3.zero, tempSat.transform.up, -1 * ((u * s) + dth * p));

                satPosition[id] = tempSat.transform.position;

                satProjection[id] = math.normalize(satPosition[id]);

                satRotation[id] = tempSat.transform.rotation;
                satID[id] = id;
                satNeighbor[id] = GetNeighbors(planeCount, satCount, p, s);

                id++;
            }
        }
        Destroy(tempSat);
    }

    private NativeArray<int> GetNeighbors(int planeCount, int satCount, int plane, int sat)
    {
        int South;
        int North;
        int East;
        int West;
        NativeArray<int> neighbors = new NativeArray<int>(6, Allocator.Temp);
        
        neighbors[0] = plane * satCount + sat;      // 0

        South = sat + 1;
        if (South > satCount - 1) { South = 0; }
        neighbors[1] = plane * satCount + South;    // 1

        North = sat - 1;
        if (North < 0) { North = satCount - 1; }
        neighbors[2] = plane * satCount + North;    // 2

        East = plane - 1;
        if (East < 0) { East = planeCount - 1; South = satCount - ctr.f + sat + 1; }
        if (South > satCount - 1) { South -= satCount; }
        neighbors[3] = East * satCount + South;      // 3

        West = plane + 1;
        if (West > planeCount - 1) { West = 0; North = sat - 1 + ctr.f; }
        if (North > satCount - 1) { North -= satCount; }
        neighbors[4] = West * satCount + North;      // 4

        return neighbors;
     }
}
        