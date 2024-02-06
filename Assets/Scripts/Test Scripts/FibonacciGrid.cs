using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FibonacciGrid : MonoBehaviour
{
    public GameObject pointPrefab; // Assign a small sphere/cube prefab
    //public int circumference = 1000;
    public int numberOfPoints = 100;
    // {
    //     get { return Mathf.CeilToInt(Mathf.Pow(circumference, 2) / Mathf.PI); }
    // } 
    public float radius = 1000;
    // {
    //     get { return circumference / (2 * Mathf.PI); }
    // }
    public float scale = 0.1f; // Scale of the prefab instances
    public float delay = 0.01f; // Delay between instantiating points

    private int prevNumPoints = 0;
    private float prevRadius = 0;
    private Vector3 prevScale = Vector3.zero;

    void Start()
    {
        StartCoroutine(AnimateFibonacciSphere());
    }

    void Update()
    {
        if (prevRadius != radius || prevScale.x != scale || numberOfPoints != prevNumPoints)
        {
            StopAllCoroutines(); // Stop the current animation if parameters have changed
            prevNumPoints = numberOfPoints;
            prevRadius = radius;
            prevScale = new Vector3(scale, scale, scale);

            ClearExistingPoints(); // Clear existing children to prevent clutter
            StartCoroutine(AnimateFibonacciSphere()); // Restart the animation with new parameters
        }
    }

    IEnumerator AnimateFibonacciSphere()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < numberOfPoints; i++)
        {
            indices.Add(i);
        }

        // Optionally shuffle the indices to randomize the order of point creation
        int n = indices.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int value = indices[k];
            indices[k] = indices[n];
            indices[n] = value;
        }

        foreach (int i in indices)
        {
            float goldenAngle = Mathf.PI * (3 - Mathf.Sqrt(5)); // ~137.5 degrees
            float theta = i * goldenAngle;
            float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / numberOfPoints);
            float x = Mathf.Sin(phi) * Mathf.Cos(theta) * radius;
            float y = Mathf.Sin(phi) * Mathf.Sin(theta) * radius;
            float z = Mathf.Cos(phi) * radius;

            Vector3 pointPosition = new Vector3(x, y, z);
            GameObject pointInstance = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);

            // Apply the desired scale to the instantiated prefab
            pointInstance.transform.localScale = prevScale;

            // Wait for the specified delay before continuing to the next point
            yield return new WaitForSeconds(delay);
        }
    }

    // Function to clear existing points
    void ClearExistingPoints()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}