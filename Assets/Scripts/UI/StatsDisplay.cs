using TMPro;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F1;
    public PlayerController player;
    public ChunkManager chunkManager;

    public TextMeshProUGUI playerStats;

    private bool statsVisible = false;
    private float deltaTime = 0.0f; // New field to track time between frames

    public void AssignEntity(PlayerController player)
    {
        this.player = player;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleStatsDisplay();
        }

        if (statsVisible && player != null)
        {
            UpdateStatsDisplay();
        }

        // Update deltaTime
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void ToggleStatsDisplay()
    {
        statsVisible = !statsVisible;
        playerStats.gameObject.SetActive(statsVisible);
    }

    void UpdateStatsDisplay()
    {
        // Calculate FPS and round to nearest whole number
        float fps = 1.0f / deltaTime;
        int roundedFPS = Mathf.RoundToInt(fps);

        // Get player coordinates and round to two decimal places
        Vector3 playerCoordinates = player.worldCoordinates;
        float roundedX = Mathf.Round(playerCoordinates.x * 10f) / 10f;
        float roundedY = Mathf.Round(playerCoordinates.y * 10f) / 10f;
        float roundedZ = Mathf.Round(playerCoordinates.z * 10f) / 10f;

        // Round velocity and other values to two decimal places
        float roundedSpeed = Mathf.Round(player.speed * 100f) / 100f;
        float roundedGravity = Mathf.Round(player.gravity * 100f) / 100f;

        // Construct the display text
        string fpsText = $"FPS: {roundedFPS}";
        string coordsText = $"X: {roundedX} Y: {roundedY} Z: {roundedZ}";
        string velocityText = $"V: {player.velocity}";
        string targetVelocity = $"tV: {player.targetVelocity}";
        playerStats.text = $"{fpsText}\n{coordsText}\n{velocityText}\n{targetVelocity}\nSpeed: {roundedSpeed}\nGravity: {roundedGravity}";
    }

}
