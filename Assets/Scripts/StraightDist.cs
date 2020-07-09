using UnityEngine;
using TMPro;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;


public class StraightDist : MonoBehaviour
{
    public Controller ctr;

    private LineRenderer lr;
    private Material straightMat;
    public TMP_Text textStraightDist;
    public TMP_Text textSVRTT;
    public TMP_Text textSFRTT;

    void Awake()
    {
        lr = GameObject.Find("StraightDist").AddComponent<LineRenderer>();
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.useWorldSpace = false;
        straightMat = new Material(Shader.Find("Unlit/Color"))
        {
            color = Color.magenta
        };
        lr.material = straightMat;
        lr.positionCount = 0;
    }


    public void DrawSlerpCurve() // Slerp = spherical interpolation.
    {
        // Takes line back to original Earth rotation reference:
        lr.transform.eulerAngles = GameObject.Find("Earth").transform.eulerAngles;
        int numberPoints = 50;
        lr.positionCount = numberPoints + 1;
        Vector3[] positions = new Vector3[numberPoints + 1];

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        float3 point0 = entityManager.GetComponentData<Translation>(ctr.startEntity).Value;
        float3 point1 = entityManager.GetComponentData<Translation>(ctr.finishEntity).Value;


        for (int i = 0; i <= numberPoints; i++)
        {
            float t = i / (float)numberPoints;
            positions[i] = Vector3.Slerp(point0, point1, t);
        }

        var rot = GameObject.Find("Earth").transform.eulerAngles;
        lr.transform.Rotate(0, -rot.y, 0);
        lr.SetPositions(positions);

        // Find the angle between the two vectors that form the two airports.
        // Same as acos(a.normalized, b.normalized) 
        float ang = Vector3.Angle(point0, point1);
        // Distance on the circle is a percentage of Earth's circunference:
        // Using 2*PI*R with R = 6371 (mean radius) although 6356km (polar radius)
        // gives us a smaller error margin going N to S than using 40008km meridional
        // or 40075km equatorial circuinference of because Earth is not a perfect
        // circle. This flattening at the poles, is called an oblate spheroid.

        float dist = (ang / 360) * 2 * Mathf.PI * ctr.earthRadius;
        float vrtt = ((2 * dist * 1000)/ ctr.Cv) * 1000;
        float frtt = (2 * dist * 1000) / ctr.Cf;

        textStraightDist.text += " is approx. " + dist.ToString("F0") + " km";
        textSVRTT.text = "Straight vacuum RTT: " + vrtt.ToString("F0") + " ms";
        textSFRTT.text = "Great circle fiber RTT: " + frtt.ToString("F0") + " ms";
    }
}
