using UnityEngine;

// MYTHRENOR'S HALLOW - DODGE MOVEMENT
//
// PURPOSE:
// Stores the direction and distance of a dodge.
// Provides movement to the existing MovementController.
//
// TODO (HUNTER INTEGRATION):
// Hunter's MovementController will apply the
// dodge displacement using CharacterController.Move.
//
// NOTE:
// This script does not move the character by itself.

public class DodgeMovement : MonoBehaviour
{
    // Dodge Movement Settings
    [SerializeField, Min(0.1f)] private float dodgeDistance = 3f;
    [SerializeField, Min(0.1f)] private float dodgeDuration = 1.1f;

    private Vector3 dodgeDirection;
    private float elapsedTime;
    private bool isMoving;

    public bool IsMoving => isMoving;

    // PURPOSE:
    // Begins a dodge in the requested world direction.
    public void BeginDodge(Vector3 direction)
    {
        // Prevent vertical movement from being applied.
        direction.y = 0f;

        // Default to forward if there is no movement input.
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = transform.forward;

            direction.y = 0f;
        }

        dodgeDirection = direction.normalized;

        elapsedTime = 0f;

        isMoving = true;
    }

    // PURPOSE:
    // Returns the displacement for one movement frame.
    // MovementController applies this using CharacterController.
    public Vector3 ConsumeFrameMovement(float deltaTime)
    {
        if (!isMoving)
        {
            return Vector3.zero;
        }

        float remainingTime = dodgeDuration - elapsedTime;

        float frameTime = Mathf.Min(deltaTime, remainingTime);

        elapsedTime += frameTime;

        if (elapsedTime >= dodgeDuration)
        {
            isMoving = false;
        }

        float dodgeSpeed = dodgeDistance / dodgeDuration;

        return dodgeDirection * dodgeSpeed * frameTime;
    }

    // PURPOSE:
    // Stops unfinished dodge displacement.
    public void CancelDodge()
    {
        isMoving = false;
    }

    private void OnDisable()
    {
        CancelDodge();
    }
}
