using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject icospherePrefab;
    public PlayerController playerPrefab;
    [Range(1000, 10000)]
    public int worldRadius = 2000;
    private Vector3 gravityCenter = Vector3.zero;

    private PlayerController playerInstance;
    private IcosphereGenerator worldGeneratorInstance;
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
        Vector3 spawnPoint = Random.onUnitSphere * (worldRadius+5);
        playerInstance = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        playerInstance.Initialize(gravityCenter, worldRadius);
    }

    // Update is called once per frame
    void Update()
    {
        worldGeneratorInstance.radius = worldRadius;
    }
}
