using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Utils
{
    public static Vector3 CalculateCenter(Vector3[] vertices)
    {
        float x = (vertices[0].x + vertices[1].x + vertices[2].x) / 3;
        float y = (vertices[0].y + vertices[1].y + vertices[2].y) / 3;
        float z = (vertices[0].z + vertices[1].z + vertices[2].z) / 3;

        return new Vector3(x, y, z);
    }
}

public interface INeighborAssignable
{
    Vector3 center {get;}
    bool AddNeighbor(Vector3 center);
}

public abstract class IcosphereBase
{
    protected int recursionLevel;

    protected float radius;
    protected Vector3 center;
    public List<Vector3> vertices {get; protected set;}
    public List<int> triangles {get; protected set;}

    protected List<float> edgeLengths;

    public IcosphereBase(int recursionLevel, float radius, Vector3 center)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        this.recursionLevel = recursionLevel;
        this.radius = radius;
        this.center = center;

        edgeLengths = new List<float>();
    }

    protected void Refine()
    {
        for(int i = 0; i < recursionLevel; i++)
        {
            edgeLengths.Clear();
            RefineTriangles();
        }
    }

    protected void AddTriangle(int v1, int v2, int v3)
    {
        triangles.Add(v1);
        triangles.Add(v2);
        triangles.Add(v3);
    }

    protected void RefineTriangles()
    {
        List<int> newTriangles = new List<int>();
        Dictionary<string, int> midPointCache = new Dictionary<string, int>(); // Use string for larger key space

        for (int i = 0; i < triangles.Count; i += 3)
        {
            int a = triangles[i];
            int b = triangles[i + 1];
            int c = triangles[i + 2];

            int ab = GetMidPointIndex(midPointCache, a, b);
            int bc = GetMidPointIndex(midPointCache, b, c);
            int ca = GetMidPointIndex(midPointCache, c, a);

            newTriangles.AddRange(new[] {a, ab, ca});
            newTriangles.AddRange(new[] {ab, b, bc});
            newTriangles.AddRange(new[] {ca, bc, c});
            newTriangles.AddRange(new[] {ab, bc, ca});

            StoreEdgeLength(vertices[a], vertices[ab]);
            StoreEdgeLength(vertices[b], vertices[bc]);
            StoreEdgeLength(vertices[c], vertices[ca]);
        }

        triangles = newTriangles; // Update the triangles list with the new set of refined triangles
    }

    private int GetMidPointIndex(Dictionary<string, int> cache, int index1, int index2)
    {
        string key = index1 < index2 ? $"{index1}-{index2}" : $"{index2}-{index1}";

        if (cache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        Vector3 middle = (vertices[index1] + vertices[index2]).normalized * radius;
        int newIndex = vertices.Count;
        vertices.Add(middle);

        cache[key] = newIndex;
        return newIndex;
    }

    protected void AssignNeighbors<T>(T settings, int v1, int v2, int v3, Dictionary<(int, int), T> edgeToChunkManager)
    where T : INeighborAssignable
    {
        var edges = new[] { (v1, v2), (v2, v3), (v3, v1) };

        foreach (var edge in edges)
        {
            if (edgeToChunkManager.TryGetValue(edge, out T neighbor))
            {
                if(settings.AddNeighbor(neighbor.center))
                {
                    neighbor.AddNeighbor(settings.center);
                }
            }
            edgeToChunkManager[edge] = settings;
        }
    }

    private void StoreEdgeLength(Vector3 vertex1, Vector3 vertex2) {
        float length = Vector3.Distance(vertex1, vertex2);
        edgeLengths.Add(length);
    }

    public float CalculateAverageEdgeLength() {
        if (edgeLengths.Count == 0) return 0;
        float totalLength = 0;
        foreach (var length in edgeLengths) {
            totalLength += length;
        }
        return totalLength / edgeLengths.Count;
    }
}