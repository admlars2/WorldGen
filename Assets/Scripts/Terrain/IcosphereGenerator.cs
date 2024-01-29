using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IcosphereGenerator : MonoBehaviour
{
    [Range(1, 8)]
    public int resolution = 1;
    public float radius = 1f;
    public float textureResolution = 1f;

    private Mesh mesh;
    private MeshCollider meshCollider;
    private int lastResolution;
    private float lastRadius;
    private float lastTextureResolution;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();
        lastResolution = resolution;
        lastRadius = radius;
        GenerateIcosphere();
    }

    private void Update()
    {
        // Check if resolution, radius, or texture resolution has changed
        if (resolution != lastResolution || !Mathf.Approximately(radius, lastRadius) 
            || !Mathf.Approximately(textureResolution, lastTextureResolution))
        {
            GenerateIcosphere();
            lastResolution = resolution;
            lastRadius = radius;
            lastTextureResolution = textureResolution; // Update last value
        }
    }

    private void GenerateIcosphere()
    {
        IcosphereCreator creator = new IcosphereCreator(resolution, radius, textureResolution);
        mesh.Clear();
        mesh.vertices = creator.Vertices.ToArray();
        mesh.triangles = creator.Triangles.ToArray();
        mesh.uv = creator.UVs.ToArray();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }
}

public class IcosphereCreator
{
    private const float TAU = Mathf.PI * 2f;
    private float PHI = (1f + Mathf.Sqrt(5f)) / 2f;

    public float Radius { get; private set; }
    public int Resolution { get; private set; }
    public float TextureResolution { get; private set;}

    public List<Vector3> Vertices { get; private set; }
    public List<int> Triangles { get; private set; }
    public List<Vector2> UVs { get; private set; }

    public IcosphereCreator(int resolution, float radius, float textureResolution)
    {
        Resolution = resolution;
        Radius = radius;
        TextureResolution = textureResolution;
        Generate();
    }

    private void Generate()
    {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        UVs = new List<Vector2>();

        // Add initial vertices
        Vertices.Add(new Vector3(-1f, PHI, 0f).normalized * Radius);
        Vertices.Add(new Vector3(1f, PHI, 0f).normalized * Radius);
        Vertices.Add(new Vector3(-1f, -PHI, 0f).normalized * Radius);
        Vertices.Add(new Vector3(1f, -PHI, 0f).normalized * Radius);
        Vertices.Add(new Vector3(0f, -1f, PHI).normalized * Radius);
        Vertices.Add(new Vector3(0f, 1f, PHI).normalized * Radius);
        Vertices.Add(new Vector3(0f, -1f, -PHI).normalized * Radius);
        Vertices.Add(new Vector3(0f, 1f, -PHI).normalized * Radius);
        Vertices.Add(new Vector3(PHI, 0f, -1f).normalized * Radius);
        Vertices.Add(new Vector3(PHI, 0f, 1f).normalized * Radius);
        Vertices.Add(new Vector3(-PHI, 0f, -1f).normalized * Radius);
        Vertices.Add(new Vector3(-PHI, 0f, 1f).normalized * Radius);

        // Add initial triangles
        AddTriangle(0, 11, 5);
        AddTriangle(0, 5, 1);
        AddTriangle(0, 1, 7);
        AddTriangle(0, 7, 10);
        AddTriangle(0, 10, 11);
        AddTriangle(1, 5, 9);
        AddTriangle(5, 11, 4);
        AddTriangle(11, 10, 2);
        AddTriangle(10, 7, 6);
        AddTriangle(7, 1, 8);
        AddTriangle(3, 9, 4);
        AddTriangle(3, 4, 2);
        AddTriangle(3, 2, 6);
        AddTriangle(3, 6, 8);
        AddTriangle(3, 8, 9);
        AddTriangle(4, 9, 5);
        AddTriangle(2, 4, 11);
        AddTriangle(6, 2, 10);
        AddTriangle(8, 6, 7);
        AddTriangle(9, 8, 1);

        // Refine triangles
        for (int i = 0; i < Resolution; i++)
        {
            RefineTriangles();
        }

        // Generate UVs
        GenerateUVs();
    }

    private void AddTriangle(int v1, int v2, int v3)
    {
        Triangles.Add(v1);
        Triangles.Add(v2);
        Triangles.Add(v3);
    }

    private void RefineTriangles()
    {
        var newTriangles = new List<int>();
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < Triangles.Count; i += 3)
        {
            int a = Triangles[i];
            int b = Triangles[i + 1];
            int c = Triangles[i + 2];

            int ab = GetMidPointIndex(midPointCache, a, b);
            int bc = GetMidPointIndex(midPointCache, b, c);
            int ca = GetMidPointIndex(midPointCache, c, a);

            newTriangles.Add(a); newTriangles.Add(ab); newTriangles.Add(ca);
            newTriangles.Add(b); newTriangles.Add(bc); newTriangles.Add(ab);
            newTriangles.Add(c); newTriangles.Add(ca); newTriangles.Add(bc);
            newTriangles.Add(ab); newTriangles.Add(bc); newTriangles.Add(ca);
        }

        Triangles = newTriangles;
    }

    private int GetMidPointIndex(Dictionary<int, int> cache, int index1, int index2)
    {
        int smallerIndex = Mathf.Min(index1, index2);
        int greaterIndex = Mathf.Max(index1, index2);
        int key = (smallerIndex << 16) + greaterIndex;

        if (cache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        Vector3 middle = (Vertices[index1] + Vertices[index2]).normalized * Radius;
        int newIndex = Vertices.Count;
        Vertices.Add(middle);

        cache[key] = newIndex;
        return newIndex;
    }

    private void GenerateUVs()
    {
        foreach (var vertex in Vertices)
        {
            Vector2 uv = new Vector2();
            uv.x = (Mathf.Atan2(vertex.x, vertex.z) / TAU + 0.5f) * TextureResolution;
            uv.y = (Mathf.Asin(vertex.y / Radius) / Mathf.PI + 0.5f) * TextureResolution;
            UVs.Add(uv);
        }
    }
}