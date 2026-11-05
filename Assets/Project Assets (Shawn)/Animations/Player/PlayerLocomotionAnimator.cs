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

    // Movement Speed Settings
    [SerializeField, Min(0.01f)] private float walkingSpeed = 3f;
    [SerializeField, Min(0.01f)] private float sprintingSpeed = 6f;

    // Animation Smoothing
    [SerializeField, Min(0f)] private float damping = 0.15f;

    private Vector3 previousPosition;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

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

        Vector3 currentPosition = transform.position;

        Vector3 displacement = currentPosition - previousPosition;

        previousPosition = currentPosition;

        // Ignore vertical movement from jumping or falling.
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
