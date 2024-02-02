using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector3 CalculateCenter(Vector3[] vertices)
    {
        float x = (vertices[0].x + vertices[1].x + vertices[2].x) / 3;
        float y = (vertices[0].y + vertices[1].y + vertices[2].y) / 3;
        float z = (vertices[0].z + vertices[1].z + vertices[2].z) / 3;

        return new Vector3(x, y, z);
    }
}
