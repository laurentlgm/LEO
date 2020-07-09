using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Entities;
using Unity.Collections;

public class SpeedSlider : MonoBehaviour
{
    public Controller ctr;
    public TMP_Text textSpeed;
    public Slider speedSlider;
    private EntityManager entityManager;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        textSpeed.text = "Speed: " + speedSlider.value.ToString("F0") + "x"; 
    }
    public void AdjustSpeed(float newSpeedMultiplier)
    {
        EntityQuery speedQuery = entityManager.CreateEntityQuery(typeof(Speed));
        NativeArray<Entity> speedEntities = speedQuery.ToEntityArray(Allocator.TempJob);
        foreach (var entity in speedEntities)
        {
            entityManager.SetComponentData<Speed>(entity, new Speed { Value = speedSlider.value });
        }
        speedEntities.Dispose();

        ctr.speedMultiplier = newSpeedMultiplier;
        textSpeed.text = "Speed: " + newSpeedMultiplier.ToString("F0") + "x";
    }
}
