using System;
using UnityEngine;

public abstract class GravityBody : MonoBehaviour
{   
    public CharacterController characterController;

    protected Vector3 worldCenter;
    protected int worldRadius;
    [SerializeField] public Vector3 worldCoordinates { get; private set; }

    private Vector3 gravityDirection;
    [SerializeField] protected float gravity = -9.81f;
    [SerializeField] public bool gravityEnabled {get; protected set;} = true;

    [SerializeField] public Vector3 velocity { get; private set; }
    [SerializeField] private float airFriction = 0.003f;
    [SerializeField] private float groundedFriction = 0.2f;
    [SerializeField] public float speed {get; private set;}
    private Vector3 lastWorldCoords = Vector3.zero;

    public void Initialize(Vector3 worldCenter, int worldRadius)
    {
        this.worldCenter = worldCenter;
        this.worldRadius = worldRadius;
    }

    protected virtual void Update()
    {
        UpdateCoordinates();
        AlignToPlanet();
    }

    protected virtual void FixedUpdate() {
        speed = ((lastWorldCoords - worldCoordinates)/Time.deltaTime).magnitude;
        lastWorldCoords = worldCoordinates;

        CalculateVelocity();
        MoveEntity();
    }

    protected void setVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }

    protected void setVelocityXZ(float x, float z)
    {
        velocity = new Vector3(x, velocity.y, z);
    }

    protected void setVelocityY(float y)
    {
        velocity = new Vector3(velocity.x, y, velocity.z);
    }

    public bool isGrounded()
    {
        return characterController.isGrounded || worldCoordinates.y <= 0.5f;
    }

    public void Accelerate(Vector3 acceleration)
    {
        velocity += acceleration * Time.deltaTime; // v = v + aΔt
    }

    private void CalculateVelocity()
    {
        bool grounded = isGrounded();
        float friction = grounded ? groundedFriction : airFriction;

        velocity *= 1-friction;

        if (gravityEnabled && !grounded)
        {
            velocity += new Vector3(0, gravity, 0) * Time.deltaTime; // v = v + aΔt
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
        // Convert latitude (φ) and longitude (θ) from degrees to radians
        float latRad = worldCoords.z * Mathf.Deg2Rad; // Latitude
        float lonRad = worldCoords.x * Mathf.Deg2Rad; // Longitude

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
        Vector3 worldDislacement = transform.TransformDirection(velocity)*Time.deltaTime; // Δt * v = Δx
        characterController.Move(worldDislacement);
    }
}