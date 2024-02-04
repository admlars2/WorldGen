using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class ChunkManagerSettings : INeighborAssignable {
    public float worldRadius;
    public Vector3 worldCenter;
    public Vector3 center { get; private set;} 
    public Vector3[] managerVertices;

    List<Vector3> neighbors;

    public ChunkManagerSettings(float radius, Vector3 center, Vector3[] managerVertices) {
        neighbors = new List<Vector3>();

        worldRadius = radius;
        worldCenter = center;

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


    [Header("Temp")]
    [Range(16, 256)]
    [SerializeField] private int renderAmount = 256; 

    [Header("Regular")]
    [SerializeField] private GameObject chunkManagerPrefab;

    private int recursionLevel;

    private float worldRadius;
    private Vector3 worldCenter;

    private Dictionary<Vector3, ChunkManager> loadedChunkManagers;
    private Dictionary<Vector3, ChunkManagerSettings> chunkManagerSettings;

    private bool generated = false;
    
    public void Initialize(int recursionLevel, float worldRadius, Vector3 worldCenter)
    {
        loadedChunkManagers = new Dictionary<Vector3, ChunkManager>();
        chunkManagerSettings = new Dictionary<Vector3, ChunkManagerSettings>();
        
        this.recursionLevel = recursionLevel;
        this.worldRadius = worldRadius;
        this.worldCenter = worldCenter;
    }

    public void GenerateChunkManagerSettings()
    {
        if(!generated)
        {
            IcosphereManagerGenerator managerGenerator = new IcosphereManagerGenerator(recursionLevel, worldRadius, worldCenter);
            chunkManagerSettings = managerGenerator.GenerateManagers();

            generated = true;
        }
    }

    public void LoadWorld() {
        List<float> managerEdgeLengths = new List<float>();
        List<float> chunkEdgeLengths = new List<float>();
        int count = 0;

        foreach (KeyValuePair<Vector3, ChunkManagerSettings> entry in chunkManagerSettings)
        {
            if (count >= renderAmount) break;

            GameObject chunkManagerObject = Instantiate(chunkManagerPrefab, Vector3.zero, Quaternion.identity, transform);
            ChunkManager chunkManager = chunkManagerObject.GetComponent<ChunkManager>();
            if (chunkManager != null) {
                chunkManager.Initialize(entry.Value.worldRadius, entry.Value.worldCenter, entry.Value.center, entry.Value.managerVertices);
                loadedChunkManagers.Add(entry.Key, chunkManager);

                chunkManager.GenerateChunks();
                chunkManager.LoadChunks();

                managerEdgeLengths.Add(chunkManager.getAverageEdgeLength());
                chunkEdgeLengths.Add(chunkManager.averageChunkLength);
            }

            count++;
        }

        if (managerEdgeLengths.Count > 0) {
            float averageManagerEdgeLength = managerEdgeLengths.Average();
            Debug.Log($"Average Manager Edge Length: {averageManagerEdgeLength}");
        } else {
            Debug.Log("No manager edge lengths to average.");
        }

        if (chunkEdgeLengths.Count > 0) {
            float averageChunkEdgeLength = chunkEdgeLengths.Average();
            Debug.Log($"Average Chunk Edge Length: {averageChunkEdgeLength}");
        } else {
            Debug.Log("No chunk edge lengths to average.");
        }

    }


    Vector3 DeterminePlayerChunk()
    {
        return Vector3.zero;
    }
}

public class IcosphereManagerGenerator : IcosphereBase
{
    private float PHI = (1f + Mathf.Sqrt(5f)) / 2f;

    public IcosphereManagerGenerator(int recursionLevel, float radius, Vector3 center) : base(recursionLevel, radius, center){}

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
        
        Refine();

        Dictionary<Vector3, ChunkManagerSettings> managerSettingsDict = new Dictionary<Vector3, ChunkManagerSettings>();
        Dictionary<(int, int), ChunkManagerSettings> edgetoChunkManager = new Dictionary<(int, int), ChunkManagerSettings>();

        for (int i = 0; i < triangles.Count; i+=3)
        {
            Vector3[] chunkVertices = new Vector3[3] {
                vertices[triangles[i]],
                vertices[triangles[i+1]],
                vertices[triangles[i+2]]
            };

            ChunkManagerSettings managerSettings = new ChunkManagerSettings(radius, center, chunkVertices);
            AssignNeighbors(managerSettings, triangles[i], triangles[i+1], triangles[i+2], edgetoChunkManager);
            managerSettingsDict.Add(managerSettings.center, managerSettings);
        }

        return managerSettingsDict;
    }
}