using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkManagerSettings {
    public int chunkManagerRecursionLevel;
    public int edgeSize;
    public float worldRadius;
    public Vector3 worldCenter;
    public Vector3 center; 
    public Vector3[] managerVertices;

    List<Vector3> neighbors;

    public ChunkManagerSettings(int chunkManagerRecursionLevel, int edgeSize, float radius, Vector3 center, Vector3[] managerVertices) {
        neighbors = new List<Vector3>();

        this.chunkManagerRecursionLevel = chunkManagerRecursionLevel;
        this.edgeSize = edgeSize;
        this.worldRadius = radius;
        this.worldCenter = center;
        this.center = Utils.CalculateCenter(managerVertices);
        this.managerVertices = managerVertices;
    }

    public bool AddNeighbor(Vector3 neighborCenter)
    {
        if(!neighbors.Contains(neighborCenter))
        {
            neighbors.Add(neighborCenter);
            return true;
        }
        return false;
    }
}


public class WorldManager : MonoBehaviour
{
    [SerializeField] private GameObject chunkManagerPrefab;

    private int recursionLevel;
    private int worldManagerRecursionLevel = 6;
    private int chunkManagerRecursionLevel
    {
        get
        {
            return recursionLevel - worldManagerRecursionLevel;
        }
    }

    private int edgeSize;
    private float worldRadius;
    private Vector3 worldCenter;

    private Dictionary<Vector3, ChunkManager> loadedChunkManagers;
    private Dictionary<Vector3, ChunkManagerSettings> chunkManagerSettings;
    
    public void Initialize(int recursionLevel, int edgeSize, float worldRadius, Vector3 worldCenter)
    {
        loadedChunkManagers = new Dictionary<Vector3, ChunkManager>();
        chunkManagerSettings = new Dictionary<Vector3, ChunkManagerSettings>();
        
        this.recursionLevel = recursionLevel;
        this.edgeSize = edgeSize;
        this.worldRadius = worldRadius;
        this.worldCenter = worldCenter;
    }

    public void GenerateChunkManagerSettings()
    {
        IcosphereManagerGenerator managerGenerator = new IcosphereManagerGenerator(worldManagerRecursionLevel, chunkManagerRecursionLevel, edgeSize, worldRadius, worldCenter);
        chunkManagerSettings = managerGenerator.GenerateManagers();
    }

    public void LoadWorld() {
        int count = 0;

        foreach (KeyValuePair<Vector3, ChunkManagerSettings> entry in chunkManagerSettings) {
            if (count >= 20) break;

            GameObject chunkManagerObject = Instantiate(chunkManagerPrefab, entry.Value.center, Quaternion.identity, transform);
            ChunkManager chunkManager = chunkManagerObject.GetComponent<ChunkManager>();
            if (chunkManager != null) {
                chunkManager.Initialize(entry.Value.chunkManagerRecursionLevel, entry.Value.edgeSize, entry.Value.worldRadius, entry.Value.worldCenter, entry.Value.managerVertices);
                loadedChunkManagers.Add(entry.Key, chunkManager);

                chunkManager.GenerateChunks();
            }

            count++;
        }
    }


    Vector3 DeterminePlayerChunk()
    {
        return Vector3.zero;
    }
}

public class IcosphereManagerGenerator
{
    private float PHI = (1f + Mathf.Sqrt(5f)) / 2f;
    
    private int recursionLevel;
    private int chunkManagerRecursionLevel;

    private float radius;
    private int edgeSize;
    private Vector3 center;

    public List<Vector3> vertices { get; private set; }
    public List<int> triangles { get; private set; }

    public IcosphereManagerGenerator(int recursionLevel, int chunkManagerRecursionLevel, int edgeSize, float radius, Vector3 center)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        this.recursionLevel = recursionLevel;
        this.chunkManagerRecursionLevel = chunkManagerRecursionLevel;
        this.radius = radius;
        this.center = center;
        this.edgeSize = edgeSize;
    }

    public Dictionary<Vector3, ChunkManagerSettings> GenerateManagers()
    {
        vertices.Clear();
        triangles.Clear();

        // Add initial vertices
        vertices.Add(new Vector3(-1f, PHI, 0f).normalized * radius);
        vertices.Add(new Vector3(1f, PHI, 0f).normalized * radius);
        vertices.Add(new Vector3(-1f, -PHI, 0f).normalized * radius);
        vertices.Add(new Vector3(1f, -PHI, 0f).normalized * radius);
        vertices.Add(new Vector3(0f, -1f, PHI).normalized * radius);
        vertices.Add(new Vector3(0f, 1f, PHI).normalized * radius);
        vertices.Add(new Vector3(0f, -1f, -PHI).normalized * radius);
        vertices.Add(new Vector3(0f, 1f, -PHI).normalized * radius);
        vertices.Add(new Vector3(PHI, 0f, -1f).normalized * radius);
        vertices.Add(new Vector3(PHI, 0f, 1f).normalized * radius);
        vertices.Add(new Vector3(-PHI, 0f, -1f).normalized * radius);
        vertices.Add(new Vector3(-PHI, 0f, 1f).normalized * radius);

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
        for (int i = 0; i < recursionLevel; i++)
        {
            RefineTriangles();
        }

        Dictionary<Vector3, ChunkManagerSettings> managerSettingsDict = new Dictionary<Vector3, ChunkManagerSettings>();
        Dictionary<(int, int), ChunkManagerSettings> edgetoChunkManager = new Dictionary<(int, int), ChunkManagerSettings>();

        for (int i = 0; i < triangles.Count; i+=3)
        {
            Vector3[] chunkVertices = new Vector3[3] {
                vertices[triangles[i]],
                vertices[triangles[i+1]],
                vertices[triangles[i+2]]
            };

            ChunkManagerSettings managerSettings = new ChunkManagerSettings(chunkManagerRecursionLevel, edgeSize, radius, center, chunkVertices);
            AssignNeighbors(managerSettings, triangles[i], triangles[i+1], triangles[i+2], edgetoChunkManager);
            managerSettingsDict.Add(managerSettings.center, managerSettings);
        }

        return managerSettingsDict;
    }

    private void AssignNeighbors(ChunkManagerSettings settings, int v1, int v2, int v3, Dictionary<(int, int), ChunkManagerSettings> edgeToChunkManager)
    {
        var edges = new[] { (v1, v2), (v2, v3), (v3, v1) };

        foreach (var edge in edges) {
            if (edgeToChunkManager.TryGetValue(edge, out ChunkManagerSettings neighbor))
            {
                if(settings.AddNeighbor(neighbor.center))
                {
                    neighbor.AddNeighbor(settings.center);
                }
            }
            edgeToChunkManager[edge] = settings;
        }
    }

    private void AddTriangle(int v1, int v2, int v3)
    {
        triangles.Add(v1);
        triangles.Add(v2);
        triangles.Add(v3);
    }

    private void RefineTriangles()
    {
        var newTriangles = new List<int>();
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            int a = triangles[i];
            int b = triangles[i + 1];
            int c = triangles[i + 2];

            int ab = GetMidPointIndex(midPointCache, a, b);
            int bc = GetMidPointIndex(midPointCache, b, c);
            int ca = GetMidPointIndex(midPointCache, c, a);

            newTriangles.Add(a); newTriangles.Add(ab); newTriangles.Add(ca);
            newTriangles.Add(b); newTriangles.Add(bc); newTriangles.Add(ab);
            newTriangles.Add(c); newTriangles.Add(ca); newTriangles.Add(bc);
            newTriangles.Add(ab); newTriangles.Add(bc); newTriangles.Add(ca);
        }

        triangles = newTriangles;
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

        Vector3 middle = (vertices[index1] + vertices[index2]).normalized * radius;
        int newIndex = vertices.Count;
        vertices.Add(middle);

        cache[key] = newIndex;
        return newIndex;
    }
}