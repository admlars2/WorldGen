using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float groundMovementSpeed = 5.0f;
    public float airMovementSpeed = 7.0f; // Faster movement speed in the air
    public float mouseSensitivity = 100.0f;
    public float upDownRange = 60.0f;
    public float verticalSpeed = 5.0f;
    public float gravity = -9.81f;
    public float jumpForce = 5.0f; // Jump force
    public float doubleTapTime = 0.3f; // Time interval for double tapping

    private float verticalRotation = 0;
    private Vector3 velocity;
    private bool isFlying = false;
    private float lastSpaceTime = -1f; // Time since last spacebar press

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();

        // Check for double-tap on space to toggle flying
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastSpaceTime < doubleTapTime)
            {
                isFlying = !isFlying; // Toggle flying mode
            }
            else if (characterController.isGrounded && !isFlying)
            {
                // Apply jump force
                velocity.y = jumpForce;
            }
            lastSpaceTime = Time.time;
        }

        if (isFlying)
        {
            HandleFlying();
        }
        else
        {
            ApplyGravity();
        }

        characterController.Move((velocity) * Time.deltaTime);
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
        float forwardSpeed = Input.GetAxis("Vertical") * currentSpeed;
        float sideSpeed = Input.GetAxis("Horizontal") * currentSpeed;
        velocity.x = sideSpeed;
        velocity.z = forwardSpeed;
        velocity = transform.rotation * velocity;
    }

    void HandleFlying()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity.y = -verticalSpeed;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            velocity.y = verticalSpeed;
        }
        else
        {
            velocity.y = 0; // Neutral vertical velocity when flying
        }
    }

    void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Max(velocity.y, 0);
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }
}