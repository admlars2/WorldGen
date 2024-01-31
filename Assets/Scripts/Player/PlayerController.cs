using System;
using UnityEngine;

public class PlayerController : Entity
{
    public float airMovementSpeed = 7.0f; // Faster movement speed in the air
    public float mouseSensitivity = 100.0f;

    private float upDownRange = 60.0f;
    private float verticalRotation = 0;
    public bool isFlying = false;
    private float lastSpaceTime = -1f; // Time since last spacebar press
    private float doubleTapTime = 0.3f; // Time interval for double tapping

    private float forwardInput = 0f;
    private float sideInput = 0f;
    private bool jumpPressed = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    protected override void Update()
    {
        base.Update();

        HandleRotation();

        // Capture input in Update
        forwardInput = Input.GetAxis("Vertical");
        sideInput = Input.GetAxis("Horizontal");
        jumpPressed = jumpPressed || Input.GetKeyDown(KeyCode.Space); // Capture jump key press
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        HandleMovement();

        if (jumpPressed)
        {
            HandleJumpAndFlying();
            jumpPressed = false; // Reset jump input
        }
    }

    private void HandleJumpAndFlying()
    {
        if (Time.time - lastSpaceTime < doubleTapTime)
        {
            isFlying = !isFlying; // Toggle flying mode
            gravityEnabled = !isFlying; // Toggle gravity to opposite of isFlying
        }
        else if (!isFlying && isGrounded())
        {
            ApplyJump();
        }
        lastSpaceTime = Time.time;
    }

    void ApplyJump()
    {
        setVelocityY(jumpForce);
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
        // Use captured input
        float currentSpeed = isFlying ? airMovementSpeed : groundMovementSpeed;

        Vector3 newVelocity = (forwardDir * forwardInput + rightDir * sideInput).normalized * currentSpeed;

        setTargetVelocityXZ(newVelocity.x, newVelocity.z);

        if (isFlying) HandleFlying();
    }

    void HandleFlying()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            setVelocityY(-airMovementSpeed);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            setVelocityY(airMovementSpeed);
        }
        else
        {
            setVelocityY(0);
        }
    }
}