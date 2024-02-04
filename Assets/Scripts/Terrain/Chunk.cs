using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private float worldRadius;
    private Vector3 worldCenter;

    public Vector3[] chunkVertices {get; private set;}
    public Vector3 center {get; private set;}
    public List<Chunk> neighbors {get; private set;}

    private List<Vector3> verts;
    private List<int> tris;
    private List<Vector2> uvs;

    private List<Color> colors;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    private bool hasGenerated = false;

    [SerializeField] private bool showGizmos = true;

    public void Initialize(float worldRadius, Vector3 worldCenter, Vector3 chunkCenter, Vector3[] vertices)
    {
        verts = new List<Vector3>();
        tris = new List<int>();
        uvs = new List<Vector2>();

        chunkVertices = vertices;
        center = chunkCenter;
        neighbors = new List<Chunk>();

        this.worldRadius = worldRadius;
        this.worldCenter = worldCenter;

        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
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
            VerticeGenerator verticeGenerator = new VerticeGenerator(worldRadius, worldCenter, chunkVertices);
            verticeGenerator.GenerateVertices();

            verts = verticeGenerator.vertices;
            tris = verticeGenerator.triangles;

            GenerateUVs();
            GenerateColors(verts.Count); // Generate colors for each vertex

            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.colors = colors.ToArray(); // Apply colors to the mesh

            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;

            hasGenerated = true;
        }
    }

    private void GenerateColors(int vertexCount)
    {
        colors = new List<Color>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
        {
            float green = Random.Range(0.1f, 1f); // Adjust these values for desired darkness
            colors.Add(new Color(0f, green, 0f));
        }
    }

    private void GenerateUVs()
    {
        uvs = new List<Vector2>(verts.Count);
        for (int i = 0; i < verts.Count; i++)
        {
            uvs.Add(new Vector2(verts[i].x, verts[i].z)); // This is a simple mapping; adjust as needed
        }
        mesh.uv = uvs.ToArray(); // Apply UVs to the mesh
    }


    private void OnDrawGizmos() {
        if (!showGizmos) return;

        if (verts == null || tris == null) return;

        Gizmos.color = Color.cyan;

        // Loop through each set of three indices in 'tris'
        for (int i = 0; i < tris.Count; i += 3) {
            if (i + 2 < tris.Count) { // Check to prevent out-of-bounds error
                Vector3 v1 = verts[tris[i]];     // First vertex of the triangle
                Vector3 v2 = verts[tris[i + 1]]; // Second vertex of the triangle
                Vector3 v3 = verts[tris[i + 2]]; // Third vertex of the triangle

                // Draw lines between the vertices
                Gizmos.DrawLine(v1, v2);
                Gizmos.DrawLine(v2, v3);
                Gizmos.DrawLine(v3, v1);
            }
        }

        Gizmos.color = Color.red;

        Vector3 cv1 = chunkVertices[0];
        Vector3 cv2 = chunkVertices[1];
        Vector3 cv3 = chunkVertices[2];

        Gizmos.DrawLine(cv1, cv2);
        Gizmos.DrawLine(cv2, cv3);
        Gizmos.DrawLine(cv3, cv1);
    }
}

public class VerticeGenerator : IcosphereBase
{
    

    public VerticeGenerator(float radius, Vector3 center, Vector3[] chunkVertices) : base(3, radius, center)
    {
        vertices = new List<Vector3>(chunkVertices);
        triangles = new List<int>{0, 1, 2};
    }

    public void GenerateVertices()
    {
        Refine();
    }
}