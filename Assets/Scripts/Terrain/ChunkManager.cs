using UnityEngine;
using System.Collections.Generic;

public class ChunkSettings : INeighborAssignable{
    public float worldRadius;
    public Vector3 worldCenter;
    public Vector3 center {get; private set;}
    public Vector3[] chunkVertices;

    public List<Vector3> neighbors;

    public ChunkSettings(float worldRadius, Vector3 worldCenter, Vector3[] chunkVertices)
    {
        this.worldRadius = worldRadius;
        this.worldCenter = worldCenter;
        center = Utils.CalculateCenter(chunkVertices);
        this.chunkVertices = chunkVertices;

        neighbors = new List<Vector3>();
    }

    public bool AddNeighbor(Vector3 neighbor)
    {
        if(!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
            return true;
        }
        return false;
    }
}

public class ChunkManager : MonoBehaviour
{
    
    [SerializeField] private GameObject chunkPrefab;

    private float worldRadius;
    private Vector3 worldCenter;
    private Vector3[] chunkManagerVertices;

    public Vector3 center {get; private set;}


    private Dictionary<Vector3, Chunk> loadedChunks;
    private Dictionary<Vector3, ChunkSettings> chunksSettings;

    public List<ChunkManager> neighborManagers;

    private bool generated = false;
    [SerializeField] private bool showGizmos = true;

    public float averageChunkLength = 0;

    public void Initialize(float worldRadius, Vector3 worldCenter, Vector3 managerCenter, Vector3[] chunkManagerVertices)
    {
        loadedChunks = new Dictionary<Vector3, Chunk>();
        chunksSettings = new Dictionary<Vector3, ChunkSettings>();
        neighborManagers = new List<ChunkManager>();

        this.worldRadius = worldRadius;
        this.worldCenter = worldCenter;
        this.chunkManagerVertices = chunkManagerVertices;

        center = managerCenter;
    }

    public void AddNeighbor(ChunkManager chunkManager)
    {
        if (!neighborManagers.Contains(chunkManager))
        {
            neighborManagers.Add(chunkManager);
            chunkManager.neighborManagers.Add(this);
        }
    }

    public void GenerateChunks() {
        if (!generated) {
            ChunkSettingsGenerator chunkSettingsGenerator = new ChunkSettingsGenerator(worldRadius, worldCenter, chunkManagerVertices);
            chunksSettings = chunkSettingsGenerator.GenerateChunks();

            averageChunkLength = chunkSettingsGenerator.CalculateAverageEdgeLength();

            generated = true;
        }
    }

    public void LoadChunks()
    {
        int count = 0;

        foreach (KeyValuePair<Vector3, ChunkSettings> entry in chunksSettings) {
            GameObject chunkManagerObject = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
            Chunk chunk = chunkManagerObject.GetComponent<Chunk>();
            if (chunk != null) {
                chunk.Initialize(entry.Value.worldRadius, entry.Value.worldCenter, entry.Value.center, entry.Value.chunkVertices);
                loadedChunks.Add(entry.Key, chunk);

                chunk.Generate();
            }

            count++;
        }
    }

    void OnDrawGizmos() {
        if (!showGizmos) return;
        // Draw original edges in yellow
        Gizmos.color = Color.yellow;

        Vector3 v1 = chunkManagerVertices[0];
        Vector3 v2 = chunkManagerVertices[1];
        Vector3 v3 = chunkManagerVertices[2];

        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v3);
        Gizmos.DrawLine(v3, v1);
    }

    public float getAverageEdgeLength() {
        // Calculate the length of each edge
        float lengthAB = Vector3.Distance(chunkManagerVertices[0], chunkManagerVertices[1]);
        float lengthBC = Vector3.Distance(chunkManagerVertices[1], chunkManagerVertices[2]);
        float lengthCA = Vector3.Distance(chunkManagerVertices[2], chunkManagerVertices[0]);

        // Calculate the average length
        return (lengthAB + lengthBC + lengthCA) / 3;
    }
}

public class ChunkSettingsGenerator : IcosphereBase
{
    public ChunkSettingsGenerator(float radius, Vector3 center, Vector3[] managerVertices) : base(2, radius, center) //RecursionLevel, worldRadius, worldCenter
    {
        vertices = new List<Vector3>(managerVertices);
        triangles = new List<int>{0, 1, 2};   
    }

    public Dictionary<Vector3, ChunkSettings> GenerateChunks()
    {
        Refine();

        Dictionary<Vector3, ChunkSettings> chunkSettingsDict = new Dictionary<Vector3, ChunkSettings>();
        Dictionary<(int, int), ChunkSettings> edgetoChunkManager = new Dictionary<(int, int), ChunkSettings>();

        for (int i = 0; i < triangles.Count; i+=3)
        {
            Vector3[] chunkVertices = new Vector3[3] {
                vertices[triangles[i]],
                vertices[triangles[i+1]],
                vertices[triangles[i+2]]
            };

            ChunkSettings managerSettings = new ChunkSettings(radius, center, chunkVertices);
            AssignNeighbors(managerSettings, triangles[i], triangles[i+1], triangles[i+2], edgetoChunkManager);
            chunkSettingsDict.Add(managerSettings.center, managerSettings);
        }

        return chunkSettingsDict;
    }
}