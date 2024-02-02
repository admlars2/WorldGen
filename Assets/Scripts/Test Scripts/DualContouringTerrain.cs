using UnityEngine;

public class DualContouringTerrain : MonoBehaviour
{

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct NodeData {
        public Vector3 center;
        public float size;
        public float density;

        // Constructor
        public NodeData(Vector3 center, float size, float density) {
            this.center = center;
            this.size = size;
            this.density = density;
        }
    }


    public ComputeShader dualContouringShader;
    private ComputeBuffer inputBuffer;

    // Parameters for the terrain
    public int initialNodeSize = 50;
    public int sphereRadius = 50;

    // Root node of the Octree
    private OctreeNode rootNode;

    void Start()
    {
        // Initialize and generate the Octree
        InitializeOctree();
        GenerateTerrain();
    }

    void InitializeOctree()
    {
        Vector3 center = new Vector3(0, 0, 0); // Center of the terrain
        rootNode = new OctreeNode(center, initialNodeSize, sphereRadius, 0);
        rootNode.RecursiveSubdivide(3);
    }

    void GenerateTerrain()
    {
        OctreeNode[] nodes = rootNode.Flatten().ToArray();
        int nodeCount = nodes.Length;

        // Assuming 32 bytes per node to be safe
        int nodeStructSize = 32;

        inputBuffer = new ComputeBuffer(nodeCount, nodeStructSize);

        // Convert the nodes to the format expected by the compute buffer
        NodeData[] nodeData = new NodeData[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            nodeData[i] = ConvertToNodeStruct(nodes[i]);
        }

        inputBuffer.SetData(nodeData);

        int kernelHandle = dualContouringShader.FindKernel("CSMain");
        dualContouringShader.SetBuffer(kernelHandle, "NodeBuffer", inputBuffer);

        // Dispatch the compute shader
        int threadGroupsX = Mathf.CeilToInt(nodeCount / 8.0f); // Adjust based on your needs
        dualContouringShader.Dispatch(kernelHandle, threadGroupsX, 1, 1);

        // Retrieve and process mesh data from the shader
        NodeData[] meshData;

        inputBuffer.Release();
    }

    void OnDestroy()
    {
        if (inputBuffer != null)
            inputBuffer.Release();
    }

    NodeData ConvertToNodeStruct(OctreeNode node)
    {
        return new NodeData(node.Center, node.Size, node.Density);
    }


    void OnDrawGizmos()
    {
        DrawNode(rootNode);
    }

    void DrawNode(OctreeNode node)
    {
        if (node == null)
        {
            return;
        }

        // Set Gizmo color and draw a cube
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(node.Center, Vector3.one * node.Size);

        // If the node has children, draw them
        if (!node.IsLeaf())
        {
            foreach (var child in node.Children)
            {
                DrawNode(child);
            }
        }
    }
}