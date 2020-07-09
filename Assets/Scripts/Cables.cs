using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using TMPro;
using UnityEngine.Networking;

public class Cables : MonoBehaviour
{
    public Controller ctr;

    private Material cableMat;
    public TMP_Dropdown dropdownFiber;
    public TMP_Text textSubmarineDist;
    public TMP_Text textSubmarineRTT;

    private List<GameObject> segmentCollection = new List<GameObject>();
    private Dictionary<string, List<List<Point>>> fiberDict = new Dictionary<string, List<List<Point>>>();


    void Start()
    {
        // Getting submarine cables direct from source:
        StartCoroutine(GetJSON());
    }


    private IEnumerator GetJSON()
    {

        string url = "https://raw.githubusercontent.com/telegeography/www.submarinecablemap.com/master/public/api/v2/cable/cable-geo.json";
        UnityWebRequest request = new UnityWebRequest(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            PaseJSON(request.downloadHandler.text);
        }
    }


    private void PaseJSON(string jsonString)
    {
        var N = JSON.Parse(jsonString);
        var cableCount = N["features"].Count;
        for (int c = 0; c < cableCount; c++)
        {
            string slug = N["features"][c]["properties"]["slug"].Value;
            fiberDict[slug] = new List<List<Point>>();
            int segmentCount = N["features"][c]["geometry"]["coordinates"].Count;

            for (int s = 0; s < segmentCount; s++)
            {
                List<Point> segment = new List<Point>();
                int points = N["features"][c]["geometry"]["coordinates"][s].Count;
                for (int p = 0; p < points; p++)
                {
                    Point point = new Point();
                    point.lat = N["features"][c]["geometry"]["coordinates"][s][p][1].AsFloat;
                    point.lon = N["features"][c]["geometry"]["coordinates"][s][p][0].AsFloat;
                    segment.Add(point);
                }
                fiberDict[slug].Add(segment);
            }
        }

        PopulateFiberDowpDown();
    }


    public class Point
    {
        public float lat;
        public float lon;
    }


    private void PopulateFiberDowpDown()
    {
        List<string> fiberList = new List<string>(fiberDict.Keys);
        fiberList.Sort();
        fiberList.Insert(0, "Select Submarine Fiber");
        dropdownFiber.AddOptions(fiberList);
    }


    public void ClearFiberCables()
    {
        for (int i =0; i < segmentCollection.Count; i++)
        {
            GameObject.Destroy(segmentCollection[i]);
        }
    }


    public void DrawFiberCable(string fiberName)
    {
        List<Vector3> dot;
        List<Vector3> positionsList;

        float x;
        float y;
        float z;
        float lat;
        float lon;
        float dist = 0f;
        float R = 0.991f;


        var segments = fiberDict[fiberName];

        for (int seg = 0; seg < segments.Count; seg++)
        {
            // Every linerenderer needs its own GameObject:
            var fiberSeg = new GameObject(fiberName + "-" + seg.ToString());
            fiberSeg.transform.parent = GameObject.Find("FiberCables").transform;
            segmentCollection.Add(fiberSeg);
            LineRenderer lr;
            lr = fiberSeg.AddComponent<LineRenderer>();

            // Fixing rotation which needs to be always -90 to match Earth:
            fiberSeg.transform.rotation = new Quaternion(0, 0, 0, 0);
            fiberSeg.transform.Rotate(0, -90, 0);

            cableMat = new Material(Shader.Find("Unlit/Color"));
            cableMat.color = Color.red;
            lr.material = cableMat;
            lr.startWidth = 0.005f;
            lr.endWidth = 0.005f;
            lr.useWorldSpace = false;

            dot = new List<Vector3>();
            positionsList = new List<Vector3>();

            for (int i = 0; i < segments[seg].Count; i++)
            {
                lat = segments[seg][i].lat * Mathf.PI / 180;
                lon = segments[seg][i].lon * Mathf.PI / 180;

                x = R * Mathf.Cos(lat) * Mathf.Cos(lon);
                y = R * Mathf.Sin(lat);
                z = R * Mathf.Cos(lat) * Mathf.Sin(lon);

                dot.Add(new Vector3(x, y, z));
            }

            for (int j = 0; j <= dot.Count - 2; j++)
            {
                for (int i = 0; i <= 4; i++)
                {
                    float t = i / (float)4;
                    positionsList.Add(Vector3.Slerp(dot[j], dot[j + 1], t));
                }
                float ang = Vector3.Angle(dot[j], dot[j + 1]);
                dist += (ang / 360) * 2 * Mathf.PI * ctr.earthRadius;
            }
            lr.positionCount = positionsList.Count;
            lr.SetPositions(positionsList.ToArray());
        }
        textSubmarineDist.text = "Selected submarine cable is " + dist.ToString("F0") + " km";
        // Calculated distance is shorter than real fiber but diff should be <5%
        float subrtt = ((2 * dist * 1000) / ctr.Cf);
        textSubmarineRTT.text = "Submarine fiber RTT: " + subrtt.ToString("F0") + " ms";
    }
}

