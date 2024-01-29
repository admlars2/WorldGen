using System;
using UnityEngine;

public class PlayerController : Entity
{
    public float groundMovementSpeed = 5.0f;
    public float airMovementSpeed = 7.0f; // Faster movement speed in the air
    public float mouseSensitivity = 100.0f;
    public float upDownRange = 60.0f;
    public float jumpForce = 5.0f; // Jump force
    public float doubleTapTime = 0.3f; // Time interval for double tapping

    private float verticalRotation = 0;
    private bool isFlying = false;
    private float lastSpaceTime = -1f; // Time since last spacebar press

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    protected override void Update()
    {
        base.Update(); // Call the base class Update method

        HandleRotation();
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleJumpAndFlying();
        }
    }

    private void HandleJumpAndFlying()
    {
        if (Time.time - lastSpaceTime < doubleTapTime)
        {
            isFlying = !isFlying; // Toggle flying mode
            gravityEnabled = !isFlying; // Toggole gravity to opposite of isFlying
        }
        else if (!isFlying)
        {
            ApplyJump();
        }
        lastSpaceTime = Time.time;
    }

    void ApplyJump()
    {
        AddVelocity(new Vector3(0, jumpForce, 0));
    }


    void HandleRotation()
    {
        float rotLeftRight = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(0, rotLeftRight, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMovement()
    {
        float currentSpeed = isFlying ? airMovementSpeed : groundMovementSpeed;
        float forwardSpeed = Input.GetAxis("Vertical");
        float sideSpeed = Input.GetAxis("Horizontal");

        Vector3 forwardDir = new Vector3(0, 0, 1);
        Vector3 rightDir = new Vector3(1, 0, 0);

        Vector3 newVelocity = (forwardDir * forwardSpeed + rightDir * sideSpeed).normalized * currentSpeed;

        AddVelocity(newVelocity);

        if (isFlying) HandleFlying();
    }

    void HandleFlying()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            AddVelocity(new Vector3(0, -airMovementSpeed, 0));
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            AddVelocity(new Vector3(0, airMovementSpeed, 0));
        }
    }
}