using TMPro;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F1; // Key to toggle the stats display
    public PlayerController player; // Reference to the Entity object

    public TextMeshProUGUI statsText; // Single UI TextMeshPro element to display all stats

    private bool statsVisible = false; // Flag to track visibility of stats

    public void AssignEntity(PlayerController player)
    {
        this.player = player;
    }

    void Update()
    {
        // Check if the designated key is pressed
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleStatsDisplay();
        }

        // Update the stats display if it's visible
        if (statsVisible && player != null)
        {
            UpdateStatsDisplay();
        }
    }

    void ToggleStatsDisplay()
    {
        // Toggle the visibility of the stats
        statsVisible = !statsVisible;
        statsText.gameObject.SetActive(statsVisible);
    }

    void UpdateStatsDisplay()
    {
        // Update the TextMeshPro text with all the current stats, each on a new line
        statsText.text = $"V: {player.velocity}\nSpeed: {player.speed}\nCoords: {player.worldCoordinates}\nGravity: {player.gravityEnabled}\nFlying: {player.isFlying}";
    }
}
