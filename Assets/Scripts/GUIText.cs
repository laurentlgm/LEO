using UnityEngine;
using TMPro;
using Unity.Entities;

public class GUIText : MonoBehaviour
{
    public Controller ctr;
    public TMP_Text textSPDist;
    public TMP_Text textSPRTT;

    public EntityManager entityManager;
    int frame;
    float correctedLength;

    void Start()
    {
        frame = 0;
        correctedLength = 0;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CreatePathEntity();
    }

    void Update()
    {
        if (frame % 10 == 0)
        {
            frame = 0;
            EntityQuery pathQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SatPath>());
            Entity pathEntity = pathQuery.GetSingletonEntity();
            SatPath satPath = entityManager.GetComponentData<SatPath>(pathEntity);
            UpdateGUIText(satPath);
        }
    }

    private void UpdateGUIText(SatPath path)
    {
        if (ctr.startEntity != Entity.Null)
        {
            if (path.Hops == 0)
            {
                textSPDist.color = Color.red;
                textSPDist.text = "Error: No satellite coverage.";
                textSPRTT.text = "";
            } else
            {
                correctedLength = path.Length * ctr.earthRadius;
                float satRTT = ((2 * correctedLength * 1000) / ctr.Cv) * 1000;

                textSPDist.color = Color.white;
                textSPDist.text = "Sat link has " + path.Hops + " hops over " + correctedLength.ToString("F0") + " km";
                textSPRTT.text = "Sat link RTT: " + satRTT.ToString("F0") + "ms";
            }
        }
    }

    private void CreatePathEntity()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype entityArchetype = entityManager.CreateArchetype(

            typeof(SatPath)
        );
        entityManager.CreateEntity(entityArchetype);
    }
}
