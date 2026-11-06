using UnityEngine;

// MYTHRENOR'S HALLOW - PLAYER LOCOMOTION ANIMATOR
//
// PURPOSE:
// Updates Idle, Walking, and Running animations
// based on the character's actual movement speed.
//
// NOTE:
// Only reads movement. Does not move the player.
//
// TODO (HUNTER INTEGRATION):
// Match walking and sprinting speeds to Hunter's
// final movement settings.

public class PlayerLocomotionAnimator : MonoBehaviour
{
    // Animator Reference
    [SerializeField] private Animator animator;

    // Movement Reference
    [SerializeField] private MovementController movementController;

    // Movement Speed Settings
    [SerializeField, Min(0.01f)] private float walkingSpeed = 3f;
    [SerializeField, Min(0.01f)] private float sprintingSpeed = 6f;

    // Animation Smoothing
    [SerializeField, Min(0f)] private float damping = 0.15f;

    private Vector3 previousPosition;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");

    // PURPOSE:
    // Retrieves Hunter's existing movement component.
    // This script only reads movement information.
    private void Awake()
    {
        if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
        }
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
    }

    // PURPOSE:
    // Reads horizontal movement without changing
    // Hunter's movement system.
    private void LateUpdate()
    {
        if (animator == null || Time.deltaTime <= 0f)
        {
            return;
        }

        // PURPOSE:
        // Measure the player's movement since last frame.
        // Hunter's MovementController already moves the character
        Vector3 currentPosition = transform.position;

        Vector3 displacement = currentPosition - previousPosition;

        previousPosition = currentPosition;

        // Calculate whether the character is rising
        // or falling based on actual vertical movement.
        float verticalSpeed = displacement.y / Time.deltaTime;

        // Read grounded state from Hunter's controller.
        bool grounded = movementController != null && movementController.isGrounded;

        // Update the Animator's airborne parameters.
        animator.SetBool(IsGroundedHash, grounded);

        animator.SetFloat(VerticalSpeedHash, grounded ? 0f : verticalSpeed);

        // Keep vertical movement out of our existing
        // Idle / Walk / Run speed calculation.
        displacement.y = 0f;

        float movementSpeed = displacement.magnitude / Time.deltaTime;

        float normalizedSpeed;

        if (movementSpeed <= walkingSpeed)
        {
            // Map standing/walking to 0.0 - 0.5.
            normalizedSpeed = Mathf.InverseLerp(0f, walkingSpeed, movementSpeed) * 0.5f;
        }

        else
        {
            // Map walking/sprinting to 0.5 - 1.0.
            normalizedSpeed = 0.5f + Mathf.InverseLerp(walkingSpeed, sprintingSpeed, movementSpeed) * 0.5f;
        }

        animator.SetFloat(SpeedHash, Mathf.Clamp01(normalizedSpeed), damping, Time.deltaTime);
    }
}
