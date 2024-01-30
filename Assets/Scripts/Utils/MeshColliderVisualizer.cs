using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class MeshColliderVisualizer : MonoBehaviour
{
    private MeshCollider meshCollider;

    void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
    }

    void OnDrawGizmos()
    {
        if (meshCollider)
        {
            Gizmos.color = Color.green; // Choose a color that is visible
            Gizmos.DrawWireMesh(meshCollider.sharedMesh, transform.position, transform.rotation, transform.localScale);
        }
    }
}
