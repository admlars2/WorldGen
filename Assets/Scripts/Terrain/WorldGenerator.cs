using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldSize {
    Tiny,
    Small,
    Medium,
    Large,
    Extreme
}

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private GameObject icospherePrefab;
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private GameObject worldManagerPrefab;

    [SerializeField] private WorldSize worldSize = WorldSize.Large;
    private int recursionLevel
    {
        get
        {
            switch (worldSize)
            {
                case WorldSize.Small:
                    return 8;
                case WorldSize.Medium:
                    return 9;
                case WorldSize.Large:
                    return 10;
                case WorldSize.Extreme:
                    return 11;
                case WorldSize.Tiny:
                default:
                    return 7;
            }
        }
    }

    [SerializeField] private int edgeLength = 12;

    private float worldRadius
    {
        get
        {
            float originalEdgeLength = edgeLength * Mathf.Pow(recursionLevel, 2);
            return originalEdgeLength/4*Mathf.Sqrt(10f + 2*Mathf.Sqrt(5f));
        }
    }
    
    private Vector3 gravityCenter = Vector3.zero;

    private PlayerController playerInstance;
    private IcosphereGenerator worldGeneratorInstance;
    private WorldManager worldManagerInstance;

    [SerializeField] private bool useStaticTerrain = false;

    [SerializeField] private StatsDisplay statsDisplay;

    private Vector3 spawnPoint;

    void Start()
    {
        CreateSpawnPoint();
        GenerateWorld();
        SpawnPlayer();
    }

    void CreateSpawnPoint()
    {
        // Angle for spawning the player on the equator
        float angle = UnityEngine.Random.Range(0f, 360f);

        // Calculate spawn point on the equator
        spawnPoint = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * worldRadius, 
            0, // Y coordinate is 0 to be on the equator
            Mathf.Sin(angle * Mathf.Deg2Rad) * worldRadius
        );

        // Adding a small offset above the surface
        spawnPoint += spawnPoint.normalized * 5;
    }

    void GenerateWorld()
    {
        if (useStaticTerrain)
        {
            GameObject world = Instantiate(icospherePrefab, gravityCenter, Quaternion.identity);
            worldGeneratorInstance = world.GetComponent<IcosphereGenerator>();
            if (worldGeneratorInstance != null)
            {
                worldGeneratorInstance.radius = worldRadius;
            }
        }
        else
        {
            GameObject world = Instantiate(worldManagerPrefab, gravityCenter, Quaternion.identity);
            worldManagerInstance = world.GetComponent<WorldManager>();
            if (worldManagerInstance != null)
            {
                worldManagerInstance.Initialize(recursionLevel, edgeLength, worldRadius, gravityCenter);
                Debug.Log("Loading");
                worldManagerInstance.GenerateChunkManagerSettings();
                worldManagerInstance.LoadWorld();
            }
        }
    }

    void SpawnPlayer()
    {
        playerInstance = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        playerInstance.Initialize(gravityCenter, worldRadius);
        statsDisplay.AssignEntity(playerInstance);
    }


    // Update is called once per frame
    void Update()
    {
        if(worldGeneratorInstance != null) worldGeneratorInstance.radius = worldRadius;
    }
}
