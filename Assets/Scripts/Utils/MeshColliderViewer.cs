using UnityEngine;

public class MeshColliderViewer : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.M;
    public MeshRenderer[] meshRenderers;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMeshColliders();
        }
    }

    void ToggleMeshColliders()
    {
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = !meshRenderer.enabled;
        }
    }
}
