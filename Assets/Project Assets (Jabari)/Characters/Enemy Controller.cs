using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float terminalVelocity = -30f;

    private CharacterController controller;

    private Vector3 moveDirection;
    private float verticalVelocity;

    public Vector3 CurrentMoveDirection => moveDirection;
    public bool IsGrounded => controller != null && controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError(
                $"{name} requires a CharacterController!",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (controller == null || !controller.enabled)
            return;

        // Maintain ground contact.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity = Mathf.Max(verticalVelocity + gravity * Time.deltaTime, terminalVelocity);
        }

        Vector3 horizontalMovement = moveDirection * moveSpeed;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Debug.Log(
                $"{name} | Direction: {moveDirection} | " +
                $"Speed: {moveSpeed} | " +
                $"Controller Enabled: {controller.enabled}"
            );
        }

        Vector3 finalMovement = horizontalMovement + Vector3.up * verticalVelocity;

        // Record position BEFORE movement.
        Vector3 beforeMove = transform.position;

        Debug.Log($"Controller: {controller != null} | " +$"Enabled: {controller.enabled} | " + $"IsGrounded: {controller.isGrounded} | " + $"Horizontal: {horizontalMovement}");

        CollisionFlags flags = controller.Move(finalMovement * Time.deltaTime);

        Debug.Log($"{name} | Collision Flags: {flags} | " + $"Velocity: {controller.velocity} | " + $"Displacement: {transform.position - beforeMove}");

        // Record position AFTER movement.
        Debug.Log($"{name} | Before: {beforeMove} | After: {transform.position}");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log(
            $"{name} collided with: {hit.collider.name} | " +
            $"Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} | " +
            $"Normal: {hit.normal}"
        );
    }

    public void SetMovement(Vector3 direction, float speed = -1f)
    {
        direction.y = 0f;

        moveDirection = Vector3.ClampMagnitude(direction, 1f);

        if (speed >= 0f)
            moveSpeed = speed;

        if (moveDirection.sqrMagnitude > 0.001f)
            transform.forward = moveDirection;
    }

    public void StopMovement()
    {
        moveDirection = Vector3.zero;
    }
}
