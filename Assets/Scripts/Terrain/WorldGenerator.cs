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
                    return 6;
                case WorldSize.Medium:
                    return 7;
                case WorldSize.Large:
                    return 8;
                case WorldSize.Extreme:
                    return 9;
                case WorldSize.Tiny:
                default:
                    return 5;
            }
        }
    }

    private int circumfrence
    {
        get
        {
            switch (worldSize)
            {
                case WorldSize.Small:
                    return 21312;
                case WorldSize.Medium:
                    return 42624;
                case WorldSize.Large:
                    return 85248;
                case WorldSize.Extreme:
                    return 170496;
                case WorldSize.Tiny:
                default:
                    return 10656; // 666 x 16
            }
        }
    } 

    private float worldRadius
    {
        get
        {
            return circumfrence / (2*Mathf.PI);
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
        // Given Cartesian coordinates
        Vector3 cartesianCoords = new Vector3(-14460, 22956, 490);

        // Normalize the vector to have a magnitude of 1, then scale it to the worldRadius
        Vector3 directionNormalized = cartesianCoords.normalized;
        spawnPoint = directionNormalized * worldRadius;

        // Since the coordinates are already on the surface, add a small offset above the surface
        spawnPoint += directionNormalized * 5; // Adjust the 5 units offset as needed
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
                worldManagerInstance.Initialize(recursionLevel, worldRadius, gravityCenter);

                worldManagerInstance.GenerateWorld();
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
