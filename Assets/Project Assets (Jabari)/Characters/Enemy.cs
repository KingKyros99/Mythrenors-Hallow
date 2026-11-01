using UnityEngine;

public class Enemy : CharacterBase
{
    [Header("Enemy AI")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private TargetingData targetingData;
    [SerializeField] private MeleeManager meleeManager;
    [SerializeField] private MeleeAttackData meleeAttack;
    [SerializeField] private float attackCheckInterval = 0.2f;

    private Transform target;
    private bool isChasing;
    private MovementController movementController;
    private CombatEntityAdapter combatEntity;
    private float nextAttackCheck;

    protected override void Awake()
    {
        movementController = GetComponent<MovementController>();
        combatEntity = GetComponent<CombatEntityAdapter>();
    }

    private void Update()
    {
        // Find the player if we don't have a target.
        if (target == null)
        {
            FindPlayer();

            if (target == null)
                StopMoving();
               
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            target.position);

        // Stop pursuing if the player leaves detection range.
        if (distance > detectionRange)
        {
            isChasing = false;
            StopMoving();
            target = null;
            return;
        }

        // Chase while the player is outside attack range.
        if (distance > attackRange)
        {
            isChasing = true;
            ChaseTarget();
        }
        else
        {
            isChasing = false;
            StopMoving();
            AttackTarget();
        }
    }

    private void FindPlayer()
    {
        if (combatEntity == null || targetingData == null)
        {
            return;
        }

        ICombatEntity nearestEnemy =
            TargetingSystem.FindNearestEnemy(combatEntity, targetingData);

        if (nearestEnemy != null)
        {
            target = nearestEnemy.CombatTransform;
        }
    }

    private void ChaseTarget()
    {
        if (!isChasing || target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f && movementController != null)
        {
            direction.Normalize();
            transform.forward = direction;
            movementController.SetAIMovement(direction);
        }
    }

    private void StopMoving()
    {
        if (movementController != null)
            movementController.SetAIMovement(Vector3.zero);
    }

    private void AttackTarget()
    {
        if (combatEntity == null || meleeManager == null || meleeAttack == null)
        {
            return;
        }

        if (Time.time < nextAttackCheck)
        {
            return;
        }

        nextAttackCheck = Time.time + attackCheckInterval;

        ICombatEntity targetEntity = target.GetComponent<CombatEntityAdapter>();

        if (targetEntity == null)
        {
            return;
        }

        meleeManager.TryAttack(combatEntity, targetEntity, meleeAttack);
    }
}
