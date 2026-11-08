using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrol,
    Chasing,
    Attacking,
    Returning,
    Disabled
}

public class Enemy : CharacterBase
{
    [Header("Enemy AI")]
    [SerializeField] private float leashDistance = 20f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private TargetingData targetingData;
    [SerializeField] private MeleeManager meleeManager;
    [SerializeField] private MeleeAttackData meleeAttack;
    [SerializeField] private float attackCheckInterval = 0.2f;
    [SerializeField] private bool canPatrol = false;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float returnSpeed = 3f;

    private Vector3 homePosition;
    private int currentPatrolIndex;

    public BossDoor bossDoor;

    private Transform target;
    private EnemyController movementController;
    private CombatEntityAdapter combatEntity;
    private float nextAttackCheck;

    [Header("AI State")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    public EnemyState CurrentState => currentState;

    protected override void Awake()
    {
        movementController = GetComponent<EnemyController>();
        combatEntity = GetComponent<CombatEntityAdapter>();
        homePosition = transform.position;
    }

    private void Update()
    {
        UpdateAI();
    }

    private void UpdateAI()
    {
        Debug.Log($"{name} | Alive: {combatEntity?.IsAlive} | State: {currentState}");

        if (combatEntity == null || !combatEntity.IsAlive)
        {
            ChangeState(EnemyState.Disabled);
            return;
        }

        UpdateState();
        HandleState();
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        OnExitState(currentState);

        currentState = newState;

        OnEnterState(currentState);
    }

    private void UpdateState()
    {
        if (currentState == EnemyState.Returning)
            return;

        if (currentState == EnemyState.Disabled)
            return;

        if (!IsTargetValid())
        {
            target = null;
            FindPlayer();

            if (!IsTargetValid())
            {
                target = null;

                if (canPatrol && patrolPoints != null && patrolPoints.Length > 0)
                {
                    ChangeState(EnemyState.Patrol);
                }
                else
                {
                    ChangeState(EnemyState.Idle);
                }

                return;
            }
        }

        float distance = Vector3.Distance(transform.position, target.position);
        float distanceFromHome = Vector3.Distance(transform.position, homePosition);

        if (distanceFromHome >= leashDistance)
        {
            target = null;
            ChangeState(EnemyState.Returning);
        }
        else if (distance <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
        }
        else
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                OnIdle();
                break;

            case EnemyState.Patrol:
                OnPatrol();
                break;

            case EnemyState.Chasing:
                OnChase();
                break;

            case EnemyState.Attacking:
                OnAttack();
                break;

            case EnemyState.Returning:
                OnReturn();
                break;

            case EnemyState.Disabled:
                StopMoving();
                break;
        }
    }

    private void FindPlayer()
    {
        if (combatEntity == null)
        {
            Debug.LogError($"{name}: CombatEntity is NULL!");
            return;
        }

        if (targetingData == null)
        {
            Debug.LogError($"{name}: TargetingData is NULL!");
            return;
        }

        ICombatEntity nearestEnemy = TargetingSystem.FindNearestEnemy(combatEntity, targetingData);

        if (nearestEnemy == null)
        {
            Debug.LogWarning($"{name}: No valid target found!");
            return;
        }

        target = nearestEnemy.CombatTransform;

        Debug.Log($"{name}: Target acquired: {target.name}");
    }

    private bool IsTargetValid()
    {
        if (target == null)
            return false;

        CombatEntityAdapter targetEntity = target.GetComponent<CombatEntityAdapter>();

        return targetEntity != null && targetEntity.IsAlive;
    }

    private void ChaseTarget()
    {
        if (target == null || movementController == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        Debug.Log(
            $"{name} | Distance: {distance:F2} | " +
            $"Direction: {direction.normalized} | " +
            $"Enemy Position: {transform.position} | " +
            $"Player Position: {target.position}"
        );

        if (direction.sqrMagnitude > 0.01f)
            movementController.SetMovement(direction.normalized, chaseSpeed);
    }

    private void StopMoving()
    {
        if (movementController != null)
            movementController.StopMovement();
    }

    private void AttackTarget()
    {
        if (target == null || combatEntity == null || meleeManager == null || meleeAttack == null)
            return;

        if (Time.time < nextAttackCheck)
            return;

        nextAttackCheck = Time.time + attackCheckInterval;

        ICombatEntity targetEntity = target.GetComponent<CombatEntityAdapter>();

        if (targetEntity == null)
            return;

        meleeManager.TryAttack(combatEntity, targetEntity, meleeAttack);
    }

    protected virtual void OnIdle()
    {
        StopMoving();
    }

    protected virtual void OnPatrol()
    {
        if (!canPatrol || patrolPoints == null || patrolPoints.Length == 0)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        Transform patrolTarget = patrolPoints[currentPatrolIndex];

        // Skip empty patrol point slots.
        if (patrolTarget == null)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            return;
        }

        Vector3 direction = patrolTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.25f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            StopMoving();
            return;
        }

        direction.Normalize();
        transform.forward = direction;

        if (movementController != null)
            movementController.SetMovement(direction, patrolSpeed);
    }

    protected virtual void OnChase()
    {
        ChaseTarget();
    }

    protected virtual void OnAttack()
    {
        StopMoving();
        AttackTarget();
    }

    protected virtual void OnReturn()
    {
        Vector3 direction = homePosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.25f)
        {
            StopMoving();
            ChangeState(EnemyState.Idle);
            return;
        }

        direction.Normalize();
        transform.forward = direction;

        if (movementController != null)
            movementController.SetMovement(direction);
    }

    protected virtual void OnEnterState(EnemyState state)
    {
        // Handle state initialization.
    }

    protected virtual void OnExitState(EnemyState state)
    {
        // Handle state cleanup.
    }
}