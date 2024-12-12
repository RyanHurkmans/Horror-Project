using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float gravity = -9.81f;

    public Animator animator; // Reference to Animator
    public Transform cameraTransform; // Reference to Camera
    public Transform headTransform; // Reference to the head

    public LayerMask groundLayer; // LayerMask for ground detection
    public BoxCollider leftBootCollider; // Reference to the left boot BoxCollider
    public BoxCollider rightBootCollider; // Reference to the right boot BoxCollider

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private float turnSmoothVelocity; // Separate variable for rotation smoothing

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController component is missing from the player object.");
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator is not assigned. Animations will not play.");
        }

        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform is not assigned. Character movement might not work as expected.");
        }

        if (headTransform == null)
        {
            Debug.LogError("Head Transform is not assigned. Camera will not follow the head.");
        }

        if (leftBootCollider == null || rightBootCollider == null)
        {
            Debug.LogError("Boot colliders are not assigned!");
        }
    }

    void Update()
    {
        GroundCheck();
        HandleMovement();
        ApplyGravity();
        UpdateCameraPosition();
    }

    private void GroundCheck()
    {
        // Use Raycast to check if the boots are touching the ground
        isGrounded = false;

        // Check if either of the boot colliders are touching the ground
        if (Physics.Raycast(leftBootCollider.transform.position, Vector3.down, 0.1f, groundLayer) ||
            Physics.Raycast(rightBootCollider.transform.position, Vector3.down, 0.1f, groundLayer))
        {
            isGrounded = true;
        }

        // Apply a slight downward force when grounded to keep the player grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Slight downward force to keep player grounded
        }
    }

    private void HandleMovement()
    {
        // Movement Input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Calculate target direction relative to the camera
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

            // Calculate movement direction based on camera
            Vector3 direction = inputDirection.z * cameraForward + inputDirection.x * cameraRight;

            // Set appropriate animation for moving forward or backward
            if (vertical < 0)
            {
                // Play backward animation without rotating the character
                PlayAnimation("Backwards");
                // Don't apply rotation smoothing for backward movement
                controller.Move(direction.normalized * walkSpeed * Time.deltaTime);  // No rotation, just move backward
            }
            else
            {
                // Play walking or running animation for forward movement
                PlayAnimation(Input.GetKey(KeyCode.LeftShift) ? "Run" : "Walk");

                // Calculate target angle for rotation (only for forward movement)
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                // Smooth rotation: rotate smoothly toward the target angle
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
                transform.rotation = Quaternion.Euler(0, smoothAngle, 0);

                // Move the player in the desired direction
                float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
                controller.Move(direction.normalized * speed * Time.deltaTime);
            }
        }
        else
        {
            // Idle animation when there's no movement
            PlayAnimation("Idle");
        }
    }

    private void ApplyGravity()
    {
        // Use manual gravity control
        if (isGrounded)
        {
            velocity.y = -2f;  // Slight downward force to keep player grounded when grounded
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;  // Apply gravity manually when not grounded
        }

        controller.Move(velocity * Time.deltaTime);  // Apply the final velocity to the CharacterController
    }

    private void UpdateCameraPosition()
    {
        if (headTransform != null && cameraTransform != null)
        {
            
            // The camera will inherit the head position and rotation.
            cameraTransform.position = headTransform.position; // Camera follows the head directly
            cameraTransform.rotation = headTransform.rotation; // Camera rotates with the head
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                animator.Play(animationName);
            }
        }
        else
        {
            Debug.LogWarning("Animator not assigned. Cannot play animation: " + animationName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the ground check in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(leftBootCollider.transform.position, leftBootCollider.size);
        Gizmos.DrawWireCube(rightBootCollider.transform.position, rightBootCollider.size);
    }
}
