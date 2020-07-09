using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitPaths : MonoBehaviour
{
    public Controller ctr;
    private GameObject[] orbits;

    void Start()
    {
        DrawOrbits(ctr.planes);
    }

    public void ResetOrbits()
    {
        foreach (var orbit in orbits)
        {
            Destroy(orbit);
        }
        DrawOrbits(ctr.planes);
    }

    private void DrawOrbits(int planeCount)
    {
        Material orbitMat = new Material(Shader.Find("Unlit/Color"));
        GameObject tempOrbit = new GameObject();

        
        orbits = new GameObject[planeCount];
        float orbitRadius = (ctr.earthRadius + ctr.altitude) / ctr.earthRadius;
        float inclRad = ctr.inclination * Mathf.Deg2Rad;

        for (int p = 0; p < planeCount; p++)
        {
            float x;
            float y;
            float z;
            float beta = 360f / planeCount;

            x = orbitRadius * Mathf.Cos(inclRad);
            y = orbitRadius * Mathf.Sin(inclRad);
            z = 0;

            int interpolationPoints = 20;
            
            Vector3[] segment = new Vector3[interpolationPoints];
            List<Vector3> positions = new List<Vector3>();
            Vector3[] point = new Vector3[4];
            GameObject tempPoint = new GameObject();

            tempPoint.transform.position = new Vector3(x, y, z);
            tempPoint.transform.Rotate(0, 0, ctr.inclination);
            tempPoint.transform.RotateAround(Vector3.zero, Vector3.up, beta * p);
            point[0] = tempPoint.transform.position;
            tempPoint.transform.RotateAround(Vector3.zero, tempPoint.transform.up, 90f);
            point[1] = tempPoint.transform.position;
            tempPoint.transform.RotateAround(Vector3.zero, tempPoint.transform.up, 90f);
            point[2] = tempPoint.transform.position;
            tempPoint.transform.RotateAround(Vector3.zero, tempPoint.transform.up, 90f);
            point[3] = tempPoint.transform.position;

            orbits[p] = Instantiate(tempOrbit);
            orbits[p].name = "Orbit-" + p;
            LineRenderer lr = orbits[p].AddComponent<LineRenderer>();
            lr.startWidth = 0.002f;
            lr.endWidth = 0.002f;
            lr.loop = true;
            lr.material = orbitMat;
            lr.material.color = Color.grey;
            lr.transform.eulerAngles = GameObject.Find("Earth").transform.eulerAngles;
            

            for (int j = 0; j < 4; j++)
            {
                int h = j + 1;
                for (int i = 0; i < interpolationPoints; i++)
                {
                    float t = i / (float)interpolationPoints;
                    if (j == 3) { h = 0; }
                    segment[i] = Vector3.Slerp(point[j], point[h], t);
                }
                positions.AddRange(segment);
            }
            lr.positionCount = positions.Count;
            lr.SetPositions(positions.ToArray());
            Destroy(tempPoint);
        }
        Destroy(tempOrbit);
    }
}
