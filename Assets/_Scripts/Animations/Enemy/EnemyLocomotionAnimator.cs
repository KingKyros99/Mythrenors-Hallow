using UnityEngine;

// MYTHRENOR'S HALLOW - ENEMY LOCOMOTION ANIMATOR
//
// PURPOSE:
// Updates an enemy's movement animations based on
// how fast the character moves through the world.
//
// NOTE:
// Reusable for different enemies.
// Does not control movement or enemy AI.

public class EnemyLocomotionAnimator : MonoBehaviour
{
    // References
    [SerializeField] private Animator animator;
    [SerializeField] private Transform movementRoot;

    // Movement Animation Settings
    [SerializeField, Min(0.1f)]
    private float runningSpeed = 4f;

    [SerializeField, Min(0f)]
    private float minimumMovementSpeed = 0.08f;

    [SerializeField, Range(0f, 1f)]
    private float smoothingTime = 0.12f;

    private Vector3 previousPosition;

    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");

    private void Awake()
    {
        // Find the enemy's Animator if not assigned.
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        // Use this GameObject as the movement root.
        if (movementRoot == null)
        {
            movementRoot = transform;
        }
    }

    private void OnEnable()
    {
        // Store the starting position.
        previousPosition = movementRoot != null ? movementRoot.position : transform.position;
    }

    private void LateUpdate()
    {
        if (animator == null || movementRoot == null || !animator.isActiveAndEnabled)
        {
            return;
        }

        if (Time.deltaTime <= 0f)
        {
            return;
        }

        // Measure how far the enemy moved this frame.
        Vector3 currentPosition = movementRoot.position;

        Vector3 movement = currentPosition - previousPosition;

        previousPosition = currentPosition;

        // Ignore vertical movement.
        movement.y = 0f;

        // Convert movement into units per second.
        float currentSpeed = movement.magnitude / Time.deltaTime;

        // Convert actual speed to the Blend Tree's 0-1 range.
        float normalizedSpeed = currentSpeed < minimumMovementSpeed ? 0f : Mathf.Clamp01(currentSpeed / Mathf.Max(0.1f, runningSpeed));

        // Smoothly update the Locomotion Blend Tree.
        animator.SetFloat(MoveSpeedHash, normalizedSpeed, smoothingTime, Time.deltaTime);
    }
}
