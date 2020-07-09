using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine.UI;

public class DrawISL : MonoBehaviour
{
    public Controller ctr;
    private int totalSats;

    private LineRenderer lr = new LineRenderer();
    Vector3[] positions;
    private EntityManager entityManager;
    private EntityQuery satQuery;

    public Toggle ISLToggle;


    void Start()
    {

        Material laserMat = new Material(Shader.Find("Unlit/Color"))
        {
            color = Color.green
        };

        lr = GameObject.Find("ISL").AddComponent<LineRenderer>();
        lr.startWidth = 0.005f;
        lr.endWidth = 0.005f;
        lr.material = laserMat;
        lr.positionCount = 0;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        if (ISLToggle.isOn)
        {
            GetSatEntities();
            DrawLasers();
        }
    }

    void GetSatEntities()
    {
        totalSats = ctr.planes * ctr.satellites;
        positions = new Vector3[totalSats +1];
        satQuery = entityManager.CreateEntityQuery(typeof(Translation), ComponentType.ReadOnly<SatID>());
    }

    void DrawLasers()
    {
        NativeArray<Entity>  satEntities = satQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<SatID> satIDs = satQuery.ToComponentDataArray<SatID>(Allocator.TempJob);
        NativeArray<Translation> satPostions = satQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        int id;
        int neiID = -1;
        int n = 1;
        DynamicBuffer<Neighbors> neighbors;

        for (int sat = 0; sat < satEntities.Length; sat++)
        {
            id = satIDs[sat].Value;
            if (id == 0)
            {
                neighbors = entityManager.GetBuffer<Neighbors>(satEntities[sat]);
                positions[0] = satPostions[sat].Value;

                while (id != neiID && n < totalSats + 1)
                {
                    positions[n] = entityManager.GetComponentData<Translation>(neighbors[4].Node).Value;
                    n++;
                    neiID = entityManager.GetComponentData<SatID>(neighbors[4].Node).Value;
                    neighbors = entityManager.GetBuffer<Neighbors>(neighbors[4].Node);
                }
            }
        }
        lr.positionCount = n;
        lr.SetPositions(positions);

        satEntities.Dispose();
        satIDs.Dispose();
        satPostions.Dispose();
    }

    public void ToggleChange()
    {
        if (!ISLToggle.isOn)
        {
            lr.positionCount = 0;
        }
    }
}
