using System;
using UnityEngine;

public class PlayerController : Entity
{
    public float airMovementSpeed = 7.0f; // Base movement speed in the air
    public float airSprintSpeed = 400.0f; // Increased movement speed in the air when sprinting
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

        HandleMovement();

        if (jumpPressed)
        {
            HandleJumpAndFlying();
            jumpPressed = false; // Reset jump input
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
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
        float currentSpeed = isFlying ? airMovementSpeed : groundMovementSpeed;

        // Check if the Ctrl key is pressed for sprinting in the air
        if (isFlying && Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = airSprintSpeed; // Use sprint speed if flying and Ctrl is pressed
        }

        Vector3 newVelocity = (forwardDir * forwardInput + rightDir * sideInput).normalized * currentSpeed;
        setTargetVelocityXZ(newVelocity.x, newVelocity.z);

        if (isFlying) HandleFlying();
    }

    void HandleFlying()
    {
        float verticalSpeed = 0;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            verticalSpeed = -airMovementSpeed;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            verticalSpeed = airMovementSpeed;
        }

        // Adjust vertical speed for sprinting in the air
        if (Input.GetKey(KeyCode.LeftControl))
        {
            verticalSpeed *= (airSprintSpeed / airMovementSpeed); // Increase vertical speed based on sprint multiplier
        }

        setVelocityY(verticalSpeed);
    }
}
