using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
public class OctreeNode
{
    public Vector3 Center {get; private set;}
    public float Size {get; private set;}
    public OctreeNode[] Children;

    public float Density {get; set;}
    public int MaterialType {get; set;}

    private int Radius;

    public OctreeNode(Vector3 center, float size, int radius, float density, int materialType = 0)
    {
        Center = center;
        Size = size;
        Children = new OctreeNode[8];

        Density = density;
        MaterialType = materialType;

        Radius = radius;
    }

    public bool IsLeaf()
    {
        return Children[0] == null;
    }

    public List<OctreeNode> Flatten()
    {
        if (Children[0].IsLeaf()) return new List<OctreeNode>(Children);

        List<OctreeNode> nodes = new List<OctreeNode>();

        foreach(var child in Children)
        {
            nodes.AddRange(child.Flatten());   
        }

        return nodes;
    }

    public void RecursiveSubdivide(int i)
    {
        Subdivide();
        if (i == 0) return;

        foreach(var child in Children)
        {
            child.RecursiveSubdivide(i - 1);
        }
    }

    public void SubdivideChildren()
    {
        if(IsLeaf()) return;

        foreach(var child in Children)
        {
            child.Subdivide();
        }
    }

    public void Subdivide()
    {
        if (!IsLeaf()) return;

        float childSize = Size / 2;
        for (int i = 0; i < 8; i++)
        {
            float x = Center.x + childSize * (i % 2 == 0 ? -0.5f : 0.5f);
            float y = Center.y + childSize * (i / 4 == 0 ? -0.5f : 0.5f);
            float z = Center.z + childSize * (i / 2 % 2 == 0 ? -0.5f : 0.5f);

            Vector3 childCenter = new Vector3(x, y, z);

            float childDensity = CalculateDensity(childCenter, Radius);
            int materialType = DetermineMaterialType(childCenter);

            Children[i] = new OctreeNode(childCenter, childSize, Radius, childDensity, materialType);
        }
    }

    private int DetermineMaterialType(Vector3 childCenter)
    {
        return 0;
    }

    public static float CalculateDensity(Vector3 childCenter, int radius)
    {
        return radius*radius - (childCenter.x*childCenter.x + childCenter.y*childCenter.y + childCenter.z * childCenter.z);
    }
}
