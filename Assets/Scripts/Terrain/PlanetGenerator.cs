using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Biome
{
    PLAINS,
    DESERT,
    OCEAN,
    
}

public struct Vertex
{
    Vector3 position;
    Vector3 normal;
    Vector3 sphereCoordinate;
    float temperature;
    float humidity;
    Biome biome;
}

public class PlanetGenerator : MonoBehaviour
{
    [Range(1, 8)]
    public int resolution = 6;
    public float radius = 1000f;
    public float textureResolution = 100f;

    private Mesh mesh;
    private MeshCollider meshCollider;
    private int lastResolution;
    private float lastRadius;
    private float lastTextureResolution;

    [SerializeField] private ComputeShader noiseShader;
    private ComputeBuffer verticeBuffer;

    [SerializeField] private int seed;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();
        lastResolution = resolution;
        lastRadius = radius;
        
        GeneratePlanet();
    }

    private void Update()
    {
        // Check if resolution, radius, or texture resolution has changed
        if (resolution != lastResolution || !Mathf.Approximately(radius, lastRadius) 
            || !Mathf.Approximately(textureResolution, lastTextureResolution))
        {
            GeneratePlanet();
            lastResolution = resolution;
            lastRadius = radius;
            lastTextureResolution = textureResolution; // Update last value
        }
    }

    private void GeneratePlanet()
    {
        PlanetIcoGenerator generator = new PlanetIcoGenerator(resolution, radius, Vector3.zero, textureResolution);
        


        mesh.Clear();
        mesh.vertices = generator.vertices.ToArray();
        mesh.triangles = generator.triangles.ToArray();
        mesh.uv = generator.uvs.ToArray();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }
}

public class PlanetIcoGenerator : IcosphereBase
{
    private const float TAU = Mathf.PI * 2f;
    private float PHI = (1f + Mathf.Sqrt(5f)) / 2f;

    public float textureResolution { get; private set;}


    public List<Vector2> uvs { get; private set; }

    public PlanetIcoGenerator(int resolution, float radius, Vector3 center, float textureResolution) : base(resolution, radius, center)
    {
        this.textureResolution = textureResolution;
        Generate();
    }

    private void Generate()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

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
        Refine();

        // Generate UVs
        GenerateUVs();
    }

    private void GenerateUVs()
    {
        foreach (var vertex in vertices)
        {
            // make compute
            Vector2 uv = new Vector2();
            uv.x = (Mathf.Atan2(vertex.x, vertex.z) / TAU + 0.5f) * textureResolution;
            uv.y = (Mathf.Asin(vertex.y / radius) / Mathf.PI + 0.5f) * textureResolution;
            uvs.Add(uv);
        }
    }
}