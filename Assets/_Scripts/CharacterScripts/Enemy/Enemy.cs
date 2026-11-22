using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Patrol,
    Chasing,
    Attacking,
    RangedAttacking,
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

    [Header("Ranged Combat")]
    [SerializeField] private RangedAttackManager rangedAttackManager;
    [SerializeField] private RangedAttackData rangedAttack;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float rangedAttackRange = 12f;

    // Ranged Line of Sight
    [SerializeField] private bool useRangedLineOfSight = false;
    [SerializeField] private LayerMask rangedObstacleLayers;

    // Optional Animated Melee
    [SerializeField] private bool useAnimatedMelee = false;
    [SerializeField] private Animator meleeAttackAnimator;

    // Animated Ranged Combat
    [SerializeField] private bool useAnimatedRanged = false;
    [SerializeField] private Animator rangedAnimator;

    private bool rangedShotPending;

    // Animated Ranged Hit Recovery
    [SerializeField] private float rangedHitRecoveryDelay = 0.3f;

    private bool waitingForRangedHitRecovery;
    private float rangedHitRecoveryUntil;
    private StatusEffectManager statusEffectManager;

    private Vector3 homePosition;
    private int currentPatrolIndex;
    private float nextRangedAttackCheck;

    public Door bossDoor;
    //Combat Audio
    [SerializeField] private bool CombatMusic = true;
    private bool isAggroed;

    private Transform target;
    private EnemyController movementController;
    private CombatEntityAdapter combatEntity;
    private float nextAttackCheck;

    [Header("AI State")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    public EnemyState CurrentState => currentState;

    public TargetingData Targeting => targetingData;
    public float AttackCheckInterval => attackCheckInterval;
    public MeleeManager MeleeManager => meleeManager;
    public MeleeAttackData MeleeAttack => meleeAttack;
    public RangedAttackManager RangedAttackManager => rangedAttackManager;
    public RangedAttackData RangedAttack => rangedAttack;
    public Transform ProjectileSpawnPoint => projectileSpawnPoint;

    public float AttackRange => attackRange;
    public float RangedAttackRange => rangedAttackRange;
    public float LeashDistance => leashDistance;

    // Return Navigation
    [SerializeField] private float returnPathRefresh = 0.5f;

    private NavMeshPath returnPath;
    private Vector3[] returnCorners;
    private int returnCornerIndex;
    private float nextReturnPathUpdate;

    // Chase Navigation
    [SerializeField] private float chasePathRefresh = 0.4f;

    private NavMeshPath chasePath;
    private Vector3[] chaseCorners;
    private int chaseCornerIndex;
    private float nextChasePathUpdate;

    public Vector3 HomePosition => homePosition;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float ReturnSpeed => returnSpeed;

    protected override void Awake()
    {
        movementController = GetComponent<EnemyController>();
        combatEntity = GetComponent<CombatEntityAdapter>();
        homePosition = transform.position;
        statusEffectManager = GetComponent<StatusEffectManager>();
        returnPath = new NavMeshPath();
        chasePath = new NavMeshPath();
    }

    private void Update()
    {
        UpdateRangedHitRecovery();

        UpdateAI();
    }

    private void UpdateRangedHitRecovery()
    {
        if (!waitingForRangedHitRecovery)
        {
            return;
        }

        if (rangedAnimator == null || !rangedAnimator.isActiveAndEnabled)
        {
            if (Time.time >= rangedHitRecoveryUntil)
            {
                waitingForRangedHitRecovery = false;
            }

            return;
        }

        AnimatorStateInfo state = rangedAnimator.GetCurrentAnimatorStateInfo(0);

        // Keep the recovery timer waiting while
        // the character is still reacting or transitioning.
        if (rangedAnimator.IsInTransition(0) || !state.IsName("Locomotion"))
        {
            rangedHitRecoveryUntil = Time.time + rangedHitRecoveryDelay;

            return;
        }

        // Only allow another shot after the recovery pause.
        if (Time.time >= rangedHitRecoveryUntil)
        {
            waitingForRangedHitRecovery = false;
        }
    }

    private void UpdateAI()
    {
        //Debug.Log($"{name} | Alive: {combatEntity?.IsAlive} | State: {currentState}");

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


        AggroMusic();
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
        else if (meleeAttack != null && distance <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
        }
        else if (rangedAttack != null && distance <= rangedAttackRange && HasRangedLineOfSight())
        {
            ChangeState(EnemyState.RangedAttacking);
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
            case EnemyState.RangedAttacking:
                OnRangedAttack();
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
            //Debug.LogError($"{name}: CombatEntity is NULL!");
            return;
        }

        if (targetingData == null)
        {
            //Debug.LogError($"{name}: TargetingData is NULL!");
            return;
        }

        ICombatEntity nearestEnemy = TargetingSystem.FindNearestEnemy(combatEntity, targetingData);

        if (nearestEnemy == null)
        {
            //Debug.LogWarning($"{name}: No valid target found!");
            return;
        }

        target = nearestEnemy.CombatTransform;

        //Debug.Log($"{name}: Target acquired: {target.name}");
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
        {
            return;
        }

        if (chasePath == null) chasePath = new NavMeshPath();

        // Update the route because the player is moving.
        if (chaseCorners == null || Time.time >= nextChasePathUpdate)
        {
            nextChasePathUpdate = Time.time + chasePathRefresh;

            bool foundStart = NavMesh.SamplePosition(transform.position, out NavMeshHit start, 1.5f, NavMesh.AllAreas);

            bool foundTarget = NavMesh.SamplePosition(target.position, out NavMeshHit destination, 1.5f, NavMesh.AllAreas);

            if (!foundStart || !foundTarget || !NavMesh.CalculatePath(start.position, destination.position,
                NavMesh.AllAreas, chasePath) || chasePath.status != NavMeshPathStatus.PathComplete)
            {
                chaseCorners = null;

                StopMoving();

                return;
            }

            chaseCorners = chasePath.corners;

            chaseCornerIndex = chaseCorners.Length > 1 ? 1 : 0;
        }

        while (chaseCornerIndex < chaseCorners.Length)
        {
            Vector3 direction = chaseCorners[chaseCornerIndex] - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.04f)
            {
                movementController.SetMovement(direction.normalized, chaseSpeed);
                return;
            }

            chaseCornerIndex++;
        }

        StopMoving();
    }
private void StopMoving()
    {
        if (movementController != null)
            movementController.StopMovement();
    }

    private void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private bool HasRangedLineOfSight()
    {
        if (target == null)
        {
            return false;
        }

        if (!useRangedLineOfSight)
        {
            return true;
        }

        Vector3 start = transform.position + Vector3.up * 0.75f;

        Vector3 end = target.position + Vector3.up * 0.9f;

        bool blocked = Physics.Linecast(start, end, rangedObstacleLayers, QueryTriggerInteraction.Ignore);

        return !blocked;
    }

    private void MeleeAttackTarget()
    {
        if (target == null || combatEntity == null || meleeManager == null || meleeAttack == null)
            return;

        if (Time.time < nextAttackCheck)
            return;

        nextAttackCheck = Time.time + attackCheckInterval;

        ICombatEntity targetEntity = target.GetComponent<CombatEntityAdapter>();

        if (targetEntity == null)
            return;

        if (useAnimatedMelee && meleeAttackAnimator != null)
        {
            // Wait until the Animator is ready for another swing.
            AnimatorStateInfo state = meleeAttackAnimator.GetCurrentAnimatorStateInfo(0);

            if (!state.IsName("Locomotion") || meleeAttackAnimator.IsInTransition(0))
            {
                return;
            }

            // Reserve the attack without applying damage yet.
            if (meleeManager.TryBeginAnimatedAttack( combatEntity, targetEntity, meleeAttack))
            {
                meleeAttackAnimator.SetTrigger("Attack");
            }
        }

        else
        {
            // Preserve the original behavior for other enemies.
            meleeManager.TryAttack(combatEntity, targetEntity, meleeAttack);
        }
    }

    private void RangedAttackTarget()
    {
        if (target == null || combatEntity == null || rangedAttackManager == null || rangedAttack == null || projectileSpawnPoint == null)
            return;

        if (Time.time < nextRangedAttackCheck)
            return;

        ICombatEntity targetEntity = target.GetComponent<CombatEntityAdapter>();

        if (targetEntity == null)
            return;

        bool fired = rangedAttackManager.TryFire( combatEntity, targetEntity, rangedAttack, projectileSpawnPoint.position);

        if (fired)
            nextRangedAttackCheck = Time.time + attackCheckInterval;
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
        FaceTarget();
        MeleeAttackTarget();
    }

    protected virtual void OnRangedAttack()
    {
        StopMoving();
        FaceTarget();

        if (useAnimatedRanged)
        {
            TryStartAnimatedRangedAttack();
        }

        else
        {
            // Keep the original firing behavior.
            RangedAttackTarget();
        }
    }

    // PURPOSE:
    // Starts the shooting animation without immediately firing.
    private void TryStartAnimatedRangedAttack()
    {
        if (waitingForRangedHitRecovery || rangedShotPending || rangedAnimator == null || !rangedAnimator.isActiveAndEnabled || target == null || combatEntity == null ||
            rangedAttackManager == null || rangedAttack == null || projectileSpawnPoint == null)
        {
            return;
        }

        if (Time.time < nextRangedAttackCheck || !rangedAttackManager.CanAttack(combatEntity, rangedAttack))
        {
            return;
        }

        CombatEntityAdapter targetEntity = target.GetComponent<CombatEntityAdapter>();

        if (targetEntity == null || !TargetingSystem.IsValidTarget(combatEntity, targetEntity, rangedAttack.targetingData))
        {
            return;
        }

        // Only start a new shot from Locomotion.
        if (rangedAnimator.IsInTransition(0) || !rangedAnimator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
        {
            return;
        }

        rangedShotPending = true;

        rangedAnimator.SetTrigger("Shoot");
    }

    // PURPOSE:
    // Called when the shooting animation reaches its firing frame.
    public void FireAnimatedRangedShot()
    {
        if (!rangedShotPending)
        {
            return;
        }

        rangedShotPending = false;

        // Do not fire if a wall blocks the player.
        if (!HasRangedLineOfSight())
        {
            return;
        }

        RangedAttackTarget();
    }

    // PURPOSE:
    // Cancels a shot if its animation is interrupted.
    public void CancelAnimatedRangedShot()
    {
        rangedShotPending = false;
    }

    protected virtual void OnReturn()
    {
        if (movementController == null)
        {
            return;
        }

        Vector3 directionHome = homePosition - transform.position;

        directionHome.y = 0f;

        // Check if the enemy reached its starting position.
        if (directionHome.sqrMagnitude < 0.25f)
        {
            StopMoving();

            ChangeState(EnemyState.Idle);

            return;
        }

        // Calculate a route around walls.
        if (returnCorners == null)
        {
            nextReturnPathUpdate = Time.time + returnPathRefresh;

            bool foundStart = NavMesh.SamplePosition(transform.position, out NavMeshHit startHit, 1.5f, NavMesh.AllAreas);

            bool foundHome = NavMesh.SamplePosition(homePosition, out NavMeshHit homeHit, 1.5f, NavMesh.AllAreas);

            if (!foundStart || !foundHome || !NavMesh.CalculatePath( startHit.position, homeHit.position, NavMesh.AllAreas, returnPath) ||
                returnPath.status != NavMeshPathStatus.PathComplete)
            {
                returnCorners = null;

                StopMoving();

                return;
            }

            returnCorners = returnPath.corners;

            returnCornerIndex = returnCorners.Length > 1 ? 1 : 0;
        }

        if (returnCorners == null)
        {
            StopMoving();

            return;
        }

        // Follow each corner of the calculated path.
        while (returnCornerIndex < returnCorners.Length)
        {
            Vector3 direction = returnCorners[returnCornerIndex] - transform.position;

            direction.y = 0f;

            // Stay close to the waypoint before turning.
            if (direction.sqrMagnitude > 0.0225f)
            {
                movementController.SetMovement(direction.normalized, returnSpeed);

                return;
            }

            returnCornerIndex++;
        }

        StopMoving();
    }

    protected virtual void OnEnterState(EnemyState state)
    {
        if (state == EnemyState.Returning)
        {
            nextReturnPathUpdate = 0f;

            returnCornerIndex = 0;

            returnCorners = null;
        }
    }

    protected virtual void OnExitState(EnemyState state)
    {
        // Handle state cleanup.
    }


    //Music Methods

    private void AggroMusic()
    {
        if (!CombatMusic || AudioManger.inst == null)
        {
            return;
        }
        bool aggroed = currentState == EnemyState.Chasing || currentState == EnemyState.Attacking;

        if (aggroed == isAggroed)
        {
            return;
        }
        isAggroed = aggroed;

        if (aggroed)
        {
            AudioManger.inst.CombatStarted();
        }
        else
        {
            AudioManger.inst.CombatEnded();
        }
    }
    private void OnEnable()
    {
        OnDirectDamageTaken += HandleRangedDamageInterrupt;
    }
    private void HandleRangedDamageInterrupt(int damage)
    {
        if (damage <= 0 || !useAnimatedRanged)
        {
            return;
        }

        // Ignore damage-over-time ticks.
        if (statusEffectManager != null && statusEffectManager.IsApplyingStatusDamage)
        {
            return;
        }

        // Cancel the unfinished shot.
        CancelAnimatedRangedShot();

        // Begin waiting for the hit reaction to finish.
        waitingForRangedHitRecovery = true;

        rangedHitRecoveryUntil = Time.time + rangedHitRecoveryDelay;

        if (rangedAnimator != null)
        {
            rangedAnimator.ResetTrigger("Shoot");
        }
    }

    private void OnDisable()
    {
        OnDirectDamageTaken -= HandleRangedDamageInterrupt;

        if (!isAggroed)
        {
            return;
        }
        isAggroed = false;
        if (AudioManger.inst != null)
        {
            AudioManger.inst.CombatEnded();
        }
    }
}
