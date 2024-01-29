using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Entity), true)]
public class EntityEditor : Editor
{
    private Vector3 teleportCoordinates = Vector3.zero;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Entity entity = (Entity)target;

        //EditorGUILayout.LabelField("Grounded", entity.characterController.isGrounded.ToString());

        // Display the velocity
        EditorGUILayout.LabelField("Velocity", entity.velocity.ToString());
        EditorGUILayout.LabelField("Speed", entity.speed.ToString());

        // Display the relative coordinates
        EditorGUILayout.LabelField("World Coordinates", entity.worldCoordinates.ToString());

        // Input fields for teleport coordinates
        teleportCoordinates = EditorGUILayout.Vector3Field("Teleport Coordinates", teleportCoordinates);

        // Button to trigger teleport
        if (GUILayout.Button("Teleport"))
        {
            Undo.RecordObject(entity, "Teleport Entity");
            entity.Teleport(teleportCoordinates);
            EditorUtility.SetDirty(entity);
        }

        // Repaint the editor if the entity is updated during play mode
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}
