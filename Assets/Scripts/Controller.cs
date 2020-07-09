using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using TMPro;

public class Controller : MonoBehaviour
{
    public InputField inp;
    public SpeedSlider sldr;
    public Airports airp;

    public int planes;
    public int satellites;
    public float inclination;
    public int f;
    public float altitude;
    public float elevation;
    public float slantDistance;
    public float earthRadius = 6371f; // Mean radius in km
    public float Cv = 299792458f; // Speed of light in m/s in vacuum
    // Speed of light in m/ms in SM fiber at 1310nm wavelength and 1.4677 refractive index.
    public float Cf = 204260000f;
    public float speedMultiplier;
    public string start = "";
    public string finish = "";
    public Entity startEntity;
    public Entity finishEntity;

    public TMP_Text textConstellation;

    private void Awake()
    {
        planes = int.Parse(inp.inputPlanes.text);
        satellites = int.Parse(inp.inputSats.text);
        inclination = int.Parse(inp.inputInclination.text);
        f = int.Parse(inp.inputPhase.text);
        altitude = int.Parse(inp.inputAltitude.text);
        elevation = int.Parse(inp.inputElevation.text);
        speedMultiplier = int.Parse(sldr.speedSlider.value.ToString("F0"));
        startEntity = new Entity();
        finishEntity = new Entity();
        UpdateWalkerDescription();
        UpdateSatFootprint();
    }

    public void UpdateWalkerDescription()
    {
        textConstellation.text = "Walker Delta " + inclination + ":" +
         (planes * satellites) + "/" + planes + "/" + f;
    }

    public void UpdateSatFootprint()
    {
        // Calculate individual satellite footprint cone based on terminal elevation.
        // Assuming a triangle between terminal A, satellite B and Earth's center C:

        float A = (90 + elevation) * Mathf.PI / 180; // angle of the triangle vertice at the observer in radians
        float B = Mathf.Asin(earthRadius * Mathf.Sin(A) / (earthRadius + altitude)); // angle of the triangle vertice at the satellite in radians
        float C = Mathf.PI - A - B; // angle of the triangle vertice at the earth's center in radians
        float d = C * earthRadius; // distance in km along the earth's surface from observer to directly under the satellite

        slantDistance = Mathf.Sqrt(d * d + altitude * altitude) / earthRadius;
    }

    public void UpdateEndpointEntities()
    {
        // Find entities to be added to every Sat once user selects start and finish:
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityQuery airportQuery = entityManager.CreateEntityQuery((ComponentType.ReadOnly<AirportIndex>()));
        NativeArray<Entity> airportEntities = airportQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<AirportIndex> airportIndex = airportQuery.ToComponentDataArray<AirportIndex>(Allocator.TempJob);

        for (int i = 0; i < airportIndex.Length; i++)
        {
            if (airp.airportName[airportIndex[i].Value] == start)
            {
                startEntity = airportEntities[i];
            }
            else if (airp.airportName[airportIndex[i].Value] == finish)
            {
                finishEntity = airportEntities[i];
            }
        }
        airportEntities.Dispose();
        airportIndex.Dispose();

        // Add start and finish entities to every sat entity:
        EntityQuery satQuery = entityManager.CreateEntityQuery(typeof(Start), typeof(Finish), ComponentType.ReadOnly<SatTag>());
        NativeArray<Entity> satEntities = satQuery.ToEntityArray(Allocator.TempJob);
        foreach (var sat in satEntities)
        {
            entityManager.SetComponentData<Start>(sat, new Start { Value = startEntity });
            entityManager.SetComponentData<Finish>(sat, new Finish { Value = finishEntity });
        }
        satEntities.Dispose();
    }
}
