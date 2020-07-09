using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using UnityEngine;

[UpdateAfter(typeof(NeighborSystem))]

public class RoutingSystem : SystemBase
{
    int frame = 0;
    LineRenderer lr;
    GameObject spf;

    List<Entity> startNeighbors;
    List<Entity> finishNeighbors;

    Entity startEntity;
    Entity finishEntity;

    ComponentDataFromEntity<Dijkstra> Dij;
    ComponentDataFromEntity<Translation> allTranslations;

    List<Entity> spfPath;
    int hopCount;
    float pathLength;

    protected override void OnCreate()
    {
        spf = new GameObject();
        spf.name = "SPF";
        spf.AddComponent<LineRenderer>();
        lr = spf.GetComponent<LineRenderer>();
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.positionCount = 0;
        lr.material = new Material(Shader.Find("Unlit/Color"))
        {
            color = Color.yellow
        };
    }

    protected override void OnUpdate()
    {
        frame++;
        if (frame % 10 == 0)
        {
            frame = 0;

            if (FindStartFinishEntities())
            {
                if (FindReacheableSats())
                {
                    Dijikstra();
                    BuildSPFPath();
                    DrawSPFPath(spfPath, allTranslations);
                    UpdateSatPathEntity(hopCount, pathLength);
                }
            }
        }
    }


    private bool FindStartFinishEntities()
    {
        EntityQuery satQuery = GetEntityQuery(ComponentType.ReadOnly<SatTag>());
        NativeArray<Entity> satArray = satQuery.ToEntityArray(Allocator.TempJob);
        ComponentDataFromEntity<Start> allStart = GetComponentDataFromEntity<Start>(true);
        ComponentDataFromEntity<Finish> allFinish = GetComponentDataFromEntity<Finish>(true);

        startEntity = allStart[satArray[0]].Value;
        finishEntity = allFinish[satArray[0]].Value;

        satArray.Dispose();

        if (startEntity != Entity.Null)
        {
            return true;
        } else
        {
            return false;
        }
    }


    private bool FindReacheableSats()
    {
        // Find sattelites covering origin and destination airports:
        startNeighbors = new List<Entity>();
        finishNeighbors = new List<Entity>();

        Entities.
        WithoutBurst().
        ForEach((in Entity sat, in CloseGroup closeGroup) =>
        {
            if (closeGroup.Value == 1)
            {
                startNeighbors.Add(sat);
            }
            else if (closeGroup.Value == 2)
            {
                finishNeighbors.Add(sat);
            }
        }).Run();

        if (startNeighbors.Count == 0 || finishNeighbors.Count == 0)
        {
            // No satelites coverage.
            lr.positionCount = 0;
            UpdateSatPathEntity(0, 0);
            return false;
        }
        else
        {
            return true;
        }
    }


    private void Dijikstra()
    {
        // List of visited nodes and min priority queue that tells us when
        // finished:
        List<Entity> visitedNodes = new List<Entity>();
        Dictionary<Entity, float> minPQ = new Dictionary<Entity, float>();
        BufferFromEntity<Neighbors> allNeighbors = GetBufferFromEntity<Neighbors>(true);

        Dij = GetComponentDataFromEntity<Dijkstra>(false);
        allTranslations = GetComponentDataFromEntity<Translation>(true);

        startNeighbors.Insert(0, startEntity);
        finishNeighbors.Insert(0, finishEntity);

        visitedNodes.Add(startEntity); // Add to visited nodes

        // Add neighbors of first node to minPQ:
        for (int i = 1; i < startNeighbors.Count; i++)
        {
            minPQ[startNeighbors[i]] = allNeighbors[startNeighbors[i]][5].Cost;

            Dijkstra tempVar;
            tempVar.SPTCost = allNeighbors[startNeighbors[i]][5].Cost;
            tempVar.PrevNode = startEntity;

            Dij[startNeighbors[i]] = tempVar;
        }


        Entity minNode = Entity.Null;

        while (minPQ.Count > 0 && !visitedNodes.Contains(finishEntity))
        {
            float minCost = 10f; // 10f = infnity here.
            foreach (var node in minPQ)
            {
                if (node.Value < minCost)
                {
                    minNode = node.Key;
                    minCost = node.Value;
                }
            }

            if (minNode == finishEntity)
            {
                break;
            }

            minPQ.Remove(minNode);
            visitedNodes.Add(minNode);

            Entity neighborName;
            float totalCost;

            for (int i = 1; i < allNeighbors[minNode].Length; i++)
            {
                if (allNeighbors[minNode][i].Node != Entity.Null)
                {
                    neighborName = allNeighbors[minNode][i].Node;
                    totalCost = Dij[minNode].SPTCost + allNeighbors[minNode][i].Cost;

                    if (!visitedNodes.Contains(neighborName))
                    {
                        if (minPQ.ContainsKey(neighborName))
                        {
                            if (totalCost < Dij[neighborName].SPTCost)
                            {
                                minPQ[neighborName] = totalCost;

                                Dijkstra tempVar;
                                tempVar.SPTCost = totalCost;
                                tempVar.PrevNode = minNode;

                                Dij[neighborName] = tempVar;
                            }
                        }
                        else
                        {
                            minPQ.Add(neighborName, totalCost);

                            Dijkstra tempVar;
                            tempVar.SPTCost = totalCost;
                            tempVar.PrevNode = minNode;

                            Dij[neighborName] = tempVar;
                        }
                    }
                }
            }
        }
    }


    private void BuildSPFPath()
    {
        //Build shortest path:
        Entity prevNode;
        Entity nextNode;

        //Starting from the end, go back on the shortest path table to
        //draw each laser link:
        prevNode = Dij[finishEntity].PrevNode;
        nextNode = finishEntity;

        string path = nextNode.Index + "-->" + prevNode.Index;
        hopCount = 0;
        pathLength = 0f;
        spfPath = new List<Entity>() { nextNode };

        while (prevNode != startEntity)
        {
            nextNode = prevNode;
            spfPath.Add(nextNode);
            prevNode = Dij[prevNode].PrevNode;
            path += "-->" + prevNode.Index;
            hopCount++;
            pathLength += math.distance(allTranslations[prevNode].Value, allTranslations[nextNode].Value);
        }
        hopCount++; // Adding on last hop for the start
        pathLength += math.distance(allTranslations[prevNode].Value, allTranslations[nextNode].Value);

        spfPath.Add(prevNode); // Adding the start Node at the end.

        //Debug.Log("shortest path:" + path);
    }


    private void DrawSPFPath(List<Entity> path, ComponentDataFromEntity<Translation> allTrans)
    {
        lr.positionCount = path.Count;
        Vector3[] positions = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            positions[i] = allTrans[path[i]].Value;
        }
        lr.SetPositions(positions);
    }


    private void UpdateSatPathEntity(int hopCount, float pathLength)
    {
        Entities.
        ForEach((ref SatPath satPath) =>
        {
            satPath.Hops = hopCount;
            satPath.Length = pathLength;
        }).Schedule();
    }
}
