using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    MeshCollider meshCollider;

    [SerializeField]
    private int resolution = 1;
    [SerializeField]
    private float radius = 1f;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshCollider = gameObject.AddComponent<MeshCollider>();

        CreateIcosphere();
        UpdateMesh();
    }

    private void CreateIcosphere()
    {
        IcosphereBuilder builder = new IcosphereBuilder();
        mesh = builder.Create(resolution, radius);
    }

    private void UpdateMesh()
    {
        meshCollider.sharedMesh = mesh;
        mesh.RecalculateBounds();
    }

    // Optional: Add a public method to update the icosphere dynamically
    public void UpdateIcosphere(int newResolution, float newRadius)
    {
        resolution = newResolution;
        radius = newRadius;
        CreateIcosphere();
        UpdateMesh();
    }

    private void OnValidate()
    {
        if (mesh != null)
        {
            CreateIcosphere();
            UpdateMesh();
        }
    }
}

public class IcosphereBuilder
{
    private struct TriangleIndices
    {
        public int v1;
        public int v2;
        public int v3;

        public TriangleIndices(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    private List<Vector3> vertices;
    private List<int> triangles;
    private Dictionary<long, int> middlePointIndexCache;

    private int index;
    private int AddVertex(Vector3 point)
    {
        vertices.Add(point.normalized);
        return index++;
    }

    private int GetMiddlePoint(int p1, int p2)
    {
        long smallerIndex = Mathf.Min(p1, p2);
        long greaterIndex = Mathf.Max(p1, p2);
        long key = (smallerIndex << 32) + greaterIndex;

        if (middlePointIndexCache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        Vector3 middle = (vertices[p1] + vertices[p2]).normalized;
        int i = AddVertex(middle);

        middlePointIndexCache.Add(key, i);
        return i;
    }

    public Mesh Create(int resolution, float radius)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        middlePointIndexCache = new Dictionary<long, int>();
        index = 0;

        // Radius
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].normalized * radius;
        }

        // Create 12 vertices of an icosahedron
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        AddVertex(new Vector3(-1f,  t,  0f));
        AddVertex(new Vector3( 1f,  t,  0f));
        AddVertex(new Vector3(-1f, -t,  0f));
        AddVertex(new Vector3( 1f, -t,  0f));

        AddVertex(new Vector3( 0f, -1f,  t));
        AddVertex(new Vector3( 0f,  1f,  t));
        AddVertex(new Vector3( 0f, -1f, -t));
        AddVertex(new Vector3( 0f,  1f, -t));

        AddVertex(new Vector3( t,  0f, -1f));
        AddVertex(new Vector3( t,  0f,  1f));
        AddVertex(new Vector3(-t,  0f, -1f));
        AddVertex(new Vector3(-t,  0f,  1f));

        // Create 20 triangles of the icosahedron
        List<TriangleIndices> faces = new List<TriangleIndices>
        {
            new TriangleIndices(0, 11, 5),
            new TriangleIndices(0, 5, 1),
            new TriangleIndices(0, 1, 7),
            new TriangleIndices(0, 7, 10),
            new TriangleIndices(0, 10, 11),
            new TriangleIndices(1, 5, 9),
            new TriangleIndices(5, 11, 4),
            new TriangleIndices(11, 10, 2),
            new TriangleIndices(10, 7, 6),
            new TriangleIndices(7, 1, 8),
            new TriangleIndices(3, 9, 4),
            new TriangleIndices(3, 4, 2),
            new TriangleIndices(3, 2, 6),
            new TriangleIndices(3, 6, 8),
            new TriangleIndices(3, 8, 9),
            new TriangleIndices(4, 9, 5),
            new TriangleIndices(2, 4, 11),
            new TriangleIndices(6, 2, 10),
            new TriangleIndices(8, 6, 7),
            new TriangleIndices(9, 8, 1)
        };

        // Refine triangles
        for (int i = 0; i < resolution; i++)
        {
            List<TriangleIndices> faces2 = new List<TriangleIndices>();
            foreach (var tri in faces)
            {
                int a = GetMiddlePoint(tri.v1, tri.v2);
                int b = GetMiddlePoint(tri.v2, tri.v3);
                int c = GetMiddlePoint(tri.v3, tri.v1);

                faces2.Add(new TriangleIndices(tri.v1, a, c));
                faces2.Add(new TriangleIndices(tri.v2, b, a));
                faces2.Add(new TriangleIndices(tri.v3, c, b));
                faces2.Add(new TriangleIndices(a, b, c));
            }
            faces = faces2;
        }

        // Convert faces to triangles
        foreach (var tri in faces)
        {
            triangles.Add(tri.v1);
            triangles.Add(tri.v2);
            triangles.Add(tri.v3);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        
        mesh.uv = GenerateUVs(vertices, radius); // Generate UVs

        mesh.RecalculateNormals();
        return mesh;
    }

    private Vector2[] GenerateUVs(List<Vector3> vertices, float radius)
    {
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            float longitude = Mathf.Atan2(vertex.x, vertex.z);
            float latitude = Mathf.Acos(vertex.y / radius);

            uvs[i] = new Vector2(1f - (longitude / Mathf.PI + 1f) / 2f, latitude / Mathf.PI);
        }
        return uvs;
    }
}
