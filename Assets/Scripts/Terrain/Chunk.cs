using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    private const float TAU = Mathf.PI * 2f;

    private int worldRadius;
    private Vector3 worldCenter;
    private int textureResolution;

    public Vector3[] vertices {get; private set;}
    public Vector3 center {get; private set;}
    public List<Chunk> neighbors {get; private set;}

    private List<Vector3> verts;
    private List<int> tris;
    private List<Vector2> uvs;

    private bool hasGenerated = false;

    public Chunk(int radius, int textureResolution, Vector3 worldCenter, Vector3[] vertices)
    {
        verts = new List<Vector3>();
        tris = new List<int>();
        uvs = new List<Vector2>();

        this.vertices = vertices;
        center = CalculateCenter();
        neighbors = new List<Chunk>();

        worldRadius = radius;
        this.worldCenter = worldCenter;
        this.textureResolution = textureResolution;
    }

    private Vector3 CalculateCenter()
    {
        float x = (vertices[0].x + vertices[1].x + vertices[2].x) / 3;
        float y = (vertices[0].y + vertices[1].y + vertices[2].y) / 3;
        float z = (vertices[0].z + vertices[1].z + vertices[2].z) / 3;

        return new Vector3(x, y, z);
    }

    public void AddNeighbor(Chunk neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
            neighbor.neighbors.Add(this);
        }
    }

    public void Generate()
    {
        if (!hasGenerated)
        {
            Subdivide();
            hasGenerated = true;
        }
    }

    void Subdivide()
    {
        // Clear existing data
        verts.Clear();
        tris.Clear();
        uvs.Clear();

        // Add original vertices
        verts.AddRange(vertices);

        // Calculate midpoints and add them
        int midpointIndex1 = AddMidpoint(vertices[0], vertices[1]);
        int midpointIndex2 = AddMidpoint(vertices[1], vertices[2]);
        int midpointIndex3 = AddMidpoint(vertices[2], vertices[0]);

        // Add new triangles using the correct indices
        AddTriangle(0, midpointIndex1, midpointIndex3);
        AddTriangle(midpointIndex1, 1, midpointIndex2);
        AddTriangle(midpointIndex1, midpointIndex2, midpointIndex3);
        AddTriangle(midpointIndex3, midpointIndex2, 2);

        UpdateUVs();
    }

    private int AddMidpoint(Vector3 a, Vector3 b)
    {
        Vector3 midpoint = (a + b) / 2;
        if (!verts.Contains(midpoint))
        {
            verts.Add(midpoint);
            return verts.Count - 1; // Return the index of the newly added midpoint
        }
        else
        {
            return verts.IndexOf(midpoint); // Return the index of the existing midpoint
        }
    }

    private void AddTriangle(int a, int b, int c)
    {
        tris.Add(a);
        tris.Add(b);
        tris.Add(c);
    }

    private void UpdateUVs()
    {
        foreach (var vertex in verts)
        {
            Vector2 uv = new Vector2();
            uv.x = (Mathf.Atan2(vertex.x, vertex.z) / TAU + 0.5f) * textureResolution;
            uv.y = (Mathf.Asin(vertex.y / worldRadius) / Mathf.PI + 0.5f) * textureResolution;
            uvs.Add(uv);
        }
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }
}
