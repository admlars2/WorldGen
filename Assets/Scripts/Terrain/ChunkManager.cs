using UnityEngine;
using System.Collections.Generic;
using System;

public class ChunkManager : MonoBehaviour {
    
    private int chunkManagerRecursionLevel;
    private int edgeSize;
    private float radius;
    private Vector3 worldCenter;
    private Vector3[] chunkManagerVertices;

    public Vector3 center {get; private set;}


    private Dictionary<Vector3, Chunk> loadedChunks;
    private Dictionary<Vector3, Chunk> chunks;

    public List<ChunkManager> neighborManagers;

    private List<Vector3> originalVertices = new List<Vector3>();
    private List<int> originalTriangles = new List<int>();
    private List<Vector3> refinedVertices = new List<Vector3>();
    private List<int> refinedTriangles = new List<int>();

    private bool generated = false;

    public void Initialize(int chunkManagerRecursionLevel, int edgeSize, float radius, Vector3 worldCenter, Vector3[] chunkManagerVertices)
    {
        loadedChunks = new Dictionary<Vector3, Chunk>();
        chunks = new Dictionary<Vector3, Chunk>();
        neighborManagers = new List<ChunkManager>();

        this.chunkManagerRecursionLevel = chunkManagerRecursionLevel;
        this.edgeSize = edgeSize;
        this.radius = radius;
        this.worldCenter = worldCenter;
        this.chunkManagerVertices = chunkManagerVertices;

        center = Utils.CalculateCenter(chunkManagerVertices); // Ensure Utils.CalculateCenter is accessible and correctly implemented
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
            originalVertices = new List<Vector3>(chunkManagerVertices);
            // Assuming your initial mesh is a single triangle. Adjust if your mesh has more triangles.
            originalTriangles = new List<int>{0, 1, 2};

            refinedVertices = new List<Vector3>(originalVertices);
            refinedTriangles = new List<int>(originalTriangles);

            Debug.Log($"Recursion Level: {chunkManagerRecursionLevel}");

            for (int i = 0; i < 1; i++) {
                // RefineTriangles should modify the refinedVertices and refinedTriangles directly
                RefineTriangles();
            }

            generated = true;
        }
    }


    private void RefineTriangles()
    {
        List<int> newTriangles = new List<int>();
        Dictionary<long, int> midPointCache = new Dictionary<long, int>(); // Use long for larger key space

        for (int i = 0; i < refinedTriangles.Count; i += 3)
        {
            int a = refinedTriangles[i];
            int b = refinedTriangles[i + 1];
            int c = refinedTriangles[i + 2];

            int ab = GetMidPointIndex(midPointCache, a, b);
            int bc = GetMidPointIndex(midPointCache, b, c);
            int ca = GetMidPointIndex(midPointCache, c, a);

            newTriangles.AddRange(new[] {a, ab, ca});
            newTriangles.AddRange(new[] {ab, b, bc});
            newTriangles.AddRange(new[] {ca, bc, c});
            newTriangles.AddRange(new[] {ab, bc, ca});
        }

        refinedTriangles = newTriangles; // Update the triangles list with the new set of refined triangles
    }

    private int GetMidPointIndex(Dictionary<long, int> cache, int index1, int index2)
    {
        long smallerIndex = Mathf.Min(index1, index2);
        long greaterIndex = Mathf.Max(index1, index2);
        long key = (smallerIndex << 32) | greaterIndex; // Create a unique key for each edge

        if (cache.TryGetValue(key, out int existingVertexIndex))
        {
            return existingVertexIndex;
        }

        Vector3 midpoint = (refinedVertices[index1] + refinedVertices[index2]) * 0.5f;
        midpoint = midpoint.normalized * radius; // Ensure the midpoint is correctly positioned on the sphere's surface
        int midpointIndex = refinedVertices.Count;
        refinedVertices.Add(midpoint);

        cache[key] = midpointIndex;
        return midpointIndex;
    }


    void OnDrawGizmos() {
        // Draw original edges in yellow

        // Draw refined edges in cyan
        Gizmos.color = Color.cyan;
        foreach (var tri in refinedTriangles) {
            if (!originalTriangles.Contains(tri)) {
                // This checks if the triangle index is part of the new (refined) triangles but not the original
                // Note: This simple check may not work correctly for all cases, especially if originalTriangles contain similar indices by coincidence
                // You may need a more sophisticated method to differentiate between original and refined triangles
                Gizmos.DrawLine(refinedVertices[tri], refinedVertices[(tri + 1) % refinedVertices.Count]);
                Gizmos.DrawLine(refinedVertices[(tri + 1) % refinedVertices.Count], refinedVertices[(tri + 2) % refinedVertices.Count]);
                Gizmos.DrawLine(refinedVertices[(tri + 2) % refinedVertices.Count], refinedVertices[tri]);
            }
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < originalTriangles.Count; i += 3) {
            Gizmos.DrawLine(originalVertices[originalTriangles[i]], originalVertices[originalTriangles[i + 1]]);
            Gizmos.DrawLine(originalVertices[originalTriangles[i + 1]], originalVertices[originalTriangles[i + 2]]);
            Gizmos.DrawLine(originalVertices[originalTriangles[i + 2]], originalVertices[originalTriangles[i]]);
        }
    }
}