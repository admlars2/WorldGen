using UnityEngine;
using System.Collections.Generic;
using System;


public class ChunkManager : MonoBehaviour {
    // Assume you have a class representing a chunk

    [SerializeField] private int textureResolution = 100;
    [SerializeField] private Material chunkMaterial;

    private int worldRadius;
    private Vector3 worldCenter;
    private Dictionary<Vector3, Chunk> loadedChunks;
    private Dictionary<Vector3, Chunk> chunks;

    void Start() {
        loadedChunks = new Dictionary<Vector3, Chunk>();
    }

    public void Initialize(int radius, Vector3 center)
    {
        worldRadius = radius;
        worldCenter = center;

        GenerateChunks();
    }

    void GenerateChunks()
    {
        IcosphereChunkGenerator chunkGenerator = new IcosphereChunkGenerator(worldRadius, textureResolution, worldCenter);
        chunks = chunkGenerator.GenerateChunks();
    }

    public void RenderChunks()
    {
        int chunkCount = 0;
        foreach (var chunkEntry in chunks)
        {
            //if (chunkCount >= 120) break;
            CreateChunkGameObject(chunkEntry.Value);
            chunkCount++;
        }
    }

    void CreateChunkGameObject(Chunk chunk)
    {
        chunk.Generate();

        GameObject chunkObject = new GameObject($"Chunk {chunk.center}");
        chunkObject.transform.parent = this.transform;

        MeshRenderer meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = chunkMaterial;

        MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshFilter.mesh = chunk.GenerateMesh();

        MeshCollider meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    void Update() {
        // Update chunk loading/unloading based on player position
    }

    void LoadChunk(Vector3 chunkId) {
        // Load or generate the chunk based on its ID
    }

    void UnloadChunk(Vector3 chunkId) {
        // Unload the chunk
    }

    string DeterminePlayerChunk() {
        // Determine which chunk the player is currently on
        return "";
    }
}

public class IcosphereChunkGenerator
{
    private float PHI = (1f + Mathf.Sqrt(5f)) / 2f;
    private const int RESOLUTION = 3;

    private int radius;
    private Vector3 center;
    private int textureResolution;

    public List<Vector3> vertices { get; private set; }
    public List<int> triangles { get; private set; }

    public IcosphereChunkGenerator(int radius, int textureResolution, Vector3 center)
    {
        this.radius = radius;
        this.textureResolution = textureResolution;
        this.center = center;
    }

    public Dictionary<Vector3, Chunk> GenerateChunks()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

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
        for (int i = 0; i < RESOLUTION; i++)
        {
            RefineTriangles();
        }

        Dictionary<Vector3, Chunk> chunks = new Dictionary<Vector3, Chunk>();
        Dictionary<(int, int), Chunk> edgeToChunk = new Dictionary<(int, int), Chunk>();

        for (int i = 0; i < triangles.Count; i+=3)
        {
            Vector3[] chunkVertices = new Vector3[3] {
                vertices[triangles[i]],
                vertices[triangles[i+1]],
                vertices[triangles[i+2]]
            };

            Chunk chunk = new Chunk(radius, textureResolution, center, chunkVertices);
            AssignNeighbors(chunk, triangles[i], triangles[i+1], triangles[i+2], edgeToChunk);
            chunks.Add(chunk.center, chunk);
        }

        return chunks;
    }

    private void AssignNeighbors(Chunk chunk, int v1, int v2, int v3, Dictionary<(int, int), Chunk> edgeToChunk)
    {
        var edges = new[] { (v1, v2), (v2, v3), (v3, v1) };

        foreach (var edge in edges) {
            if (edgeToChunk.TryGetValue(edge, out Chunk neighbor))
            {
                chunk.AddNeighbor(neighbor);
            }
            edgeToChunk[edge] = chunk;
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