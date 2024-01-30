using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject icospherePrefab;
    public PlayerController playerPrefab;
    [Range(1000, 10000)]
    public int worldRadius = 10000;
    private Vector3 gravityCenter = Vector3.zero;

    private PlayerController playerInstance;
    private IcosphereGenerator worldGeneratorInstance;

    [SerializeField] private StatsDisplay statsDisplay;
    // Start is called before the first frame update
    void Start()
    {
        GenerateWorld();
        SpawnPlayer();
    }

    void GenerateWorld()
    {
        GameObject world = Instantiate(icospherePrefab, gravityCenter, Quaternion.identity);
        worldGeneratorInstance = world.GetComponent<IcosphereGenerator>();
        if (worldGeneratorInstance != null)
        {
            worldGeneratorInstance.radius = worldRadius;
        }
    }

    void SpawnPlayer()
    {
        // Angle for spawning the player on the equator
        float angle = Random.Range(0f, 360f);

        // Calculate spawn point on the equator
        Vector3 spawnPoint = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * worldRadius, 
            0, // Y coordinate is 0 to be on the equator
            Mathf.Sin(angle * Mathf.Deg2Rad) * worldRadius
        );

        // Adding a small offset above the surface
        spawnPoint += spawnPoint.normalized * 5;

        playerInstance = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        playerInstance.Initialize(gravityCenter, worldRadius);
        statsDisplay.AssignEntity(playerInstance);
    }


    // Update is called once per frame
    void Update()
    {
        worldGeneratorInstance.radius = worldRadius;
    }
}
