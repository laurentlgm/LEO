using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;

public class Airports : MonoBehaviour
{
    public Controller ctr;

    public TMP_Dropdown dropdownStart;
    public TMP_Dropdown dropdownFinish;

    private List<float3> airportPosition;
    private List<string> startList;
    private List<string> finishList;
    public List<string> airportName;

    private JSONNode airportsJSON;
    public JSONNode pingAvg;

   
    // Start is called before the first frame update
    void Start()
    {
        airportPosition = new List<float3>(); // For the Entities.
        airportName = new List<string>();

        StartCoroutine(LoadStreamingAsset("airports.json"));
        StartCoroutine(LoadStreamingAsset("citylatencies.json"));
    }


    // Load Airports/Cities and Latencies from JSON files:
    IEnumerator LoadStreamingAsset(string fileName)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        JSONNode node = null;
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            UnityWebRequest request = new UnityWebRequest(filePath);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                node = PaseJSON(request.downloadHandler.text);
            }
        }
        else
        {
            node = PaseJSON(System.IO.File.ReadAllText(filePath));
        }

        if (fileName.Contains("airports.json"))
        {
            airportsJSON = node;
            CalcAirportPositions();
            CreateAirportEntities();
            PopulateDropdowns();
        }
        else
        {
            pingAvg = node;
        }   
    }


    private JSONNode PaseJSON(string jsonString)
    {
        return JSON.Parse(jsonString);
    }


    //Helper functon for Degrees and Direction to Decimal Degrees:
    float DDToDD(float degrees, string direction)
    {
        var dd = degrees;

        if (direction == "S" || direction == "W")
        {
            dd *= -1;
        }
        return dd * Mathf.PI / 180;
    }


    // Calculate x,y,z from lat,lon
    public void CalcAirportPositions()
    {
        foreach (var location in airportsJSON.Keys)
        {
            float x;
            float y;
            float z;
            float lat;
            float lon;
            float R = 0.991f; // Not sure what changed that the points ar not on surface.

            lat = DDToDD(airportsJSON[location]["latdeg"], airportsJSON[location]["latdir"]);
            lon = DDToDD(airportsJSON[location]["londeg"], airportsJSON[location]["londir"]);

            x = R * Mathf.Cos(lat) * Mathf.Cos(lon);
            y = R * Mathf.Sin(lat);
            z = R * Mathf.Cos(lat) * Mathf.Sin(lon);

            airportPosition.Add(new float3(x, y, z));
            airportName.Add(airportsJSON[location]["city"]);
        }
    }


    public void CreateAirportEntities()
    {
        
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = sphere.GetComponent<MeshFilter>().mesh;
        Destroy(sphere);
        Material material = new Material(Shader.Find("Unlit/Color"))
        {
            enableInstancing = true,
            color = Color.red
        };

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype entityArchetype = entityManager.CreateArchetype(

            typeof(AirportTag),
            typeof(AirportIndex),
            typeof(Dijkstra),
            typeof(Translation),
            typeof(Speed),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderBounds),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(airportsJSON.Count, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            entityManager.SetComponentData(entity, new Scale { Value = 0.01f });
            entityManager.SetSharedComponentData(entity, new RenderMesh { mesh = mesh, material = material });

            entityManager.SetComponentData(entity, new Speed { Value = ctr.speedMultiplier });

            entityManager.SetComponentData(entity, new Translation { Value = airportPosition[i] });
            entityManager.SetComponentData(entity, new AirportIndex { Value = i });

            // Dijkstra specific:
            entityManager.SetComponentData(entity, new Dijkstra { SPTCost = 0f, PrevNode = entity });
        }
        entityArray.Dispose();
        airportPosition.Clear();
    }

    void PopulateDropdowns()
    {
        startList = new List<string>(airportName);
        finishList = new List<string>(airportName);
        startList.Sort();
        finishList.Sort();
        startList.Insert(0, "Select Origin");
        finishList.Insert(0, "Select Destination");

        dropdownStart.AddOptions(startList);
        dropdownFinish.AddOptions(finishList);
    }
}
