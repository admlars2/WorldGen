using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected Vector3 worldCenter;
    protected int worldRadius;
    public Vector3 worldCoordinates { get; private set; }

    public Vector3 gravityDirection { get; private set; }
    [SerializeField] protected float gravity = -9.81f;
    public CharacterController characterController { get; private set;}

    protected bool gravityEnabled = true;


    public Vector3 velocity { get; private set; }
    public float velocityDampening = .997f;
    public float speed {get; private set;}
    private Vector3 lastWorldCoords = Vector3.zero; 

    public void Initialize(Vector3 worldCenter, int worldRadius)
    {
        this.worldCenter = worldCenter;
        this.worldRadius = worldRadius;

        characterController = GetComponent<CharacterController>();
    }

    protected virtual void Update()
    {
        UpdateCoordinates();
        AlignToPlanet();
    }

    protected void FixedUpdate() {
        speed = ((lastWorldCoords - worldCoordinates)/Time.deltaTime).magnitude;
        lastWorldCoords = worldCoordinates;

        dampenVelocity();
        ApplyGravity();
        MoveEntity();
    }

    public void AddVelocity(Vector3 newVelocity)
    {
        velocity += newVelocity;
    }

    private void dampenVelocity()
    {
        velocity *= velocityDampening;
    }

    private void ApplyGravity()
    {
        if (gravityEnabled && !characterController.isGrounded && worldCoordinates.y > 0.5f)
        {
            AddVelocity(new Vector3(0, gravity, 0));
        }
    }

    private void AlignToPlanet()
    {
        Quaternion toRotation = Quaternion.FromToRotation(transform.up, gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 50 * Time.deltaTime);
    }
    private void UpdateCoordinates()
    {
        gravityDirection = (transform.position - worldCenter).normalized;
        float distanceToCenter = Vector3.Distance(transform.position, worldCenter);

        // Calculating latitude and longitude
        float latitude = Mathf.Asin(gravityDirection.y) * Mathf.Rad2Deg; // -90 to 90 degrees
        float longitude = Mathf.Atan2(gravityDirection.z, gravityDirection.x) * Mathf.Rad2Deg; // -180 to 180 degrees

        // Scaling factors (assuming circumference = 2 * π * radius)
        float scaleFactor = 180f / Mathf.PI;

        // Scale the latitude and longitude
        latitude *= scaleFactor;
        longitude *= scaleFactor;

        // Vertical distance from the surface
        float verticalDistance = distanceToCenter - worldRadius;

        // Assign the calculated values to relativeCoordinates
        worldCoordinates = new Vector3(longitude, verticalDistance, latitude);
    }


    public void Teleport(Vector3 worldCoords)
    {
        // Convert latitude and longitude from degrees to radians
        float latRad = worldCoords.x * Mathf.PI / 180f;
        float lonRad = worldCoords.z * Mathf.PI / 180f;

        // Calculate the radius at the entity's position
        float radiusAtPosition = worldRadius + worldCoords.y;

        // Convert spherical coordinates to Cartesian coordinates
        float x = radiusAtPosition * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        float y = radiusAtPosition * Mathf.Sin(latRad);
        float z = radiusAtPosition * Mathf.Cos(latRad) * Mathf.Sin(lonRad);

        transform.position = new Vector3(x, y, z) + worldCenter;
    }


    private void MoveEntity()
    {
        Vector3 worldDislacement = transform.TransformDirection(velocity);
        characterController.Move(worldDislacement*Time.deltaTime);
    }
}