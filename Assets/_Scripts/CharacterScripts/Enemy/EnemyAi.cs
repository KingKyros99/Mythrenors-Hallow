using UnityEngine;

[RequireComponent(typeof(BehaviourTree))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Reference")]
    [SerializeField] private Enemy enemy;

    private BehaviourTree behaviourTree;
    private float nextMeleeAttackTime;
    private float nextRangedAttackTime;

    public EnemyContext Context { get; private set; }

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponent<Enemy>();

        behaviourTree = GetComponent<BehaviourTree>();

        if (enemy == null)
        {
            //Debug.LogError($"{name}: Enemy component missing!", this);
            enabled = false;
            return;
        }

        Context = new EnemyContext(enemy);
        InitializeTree(BuildBehaviourTree());
    }

    public void InitializeTree(Node rootNode)
    {
        if (rootNode == null)
        {
            //Debug.LogError($"{name}: Cannot initialize an empty Behaviour Tree!", this);
            return;
        }

        behaviourTree.Initialize(rootNode);
    }

    private NodeState FindTarget(EnemyContext ctx)
    {
        if (ctx.Combat == null || ctx.Targeting == null)
            return NodeState.FAILURE;

        // Keep the current target if it is still alive.
        if (ctx.Target != null)
        {
            CombatEntityAdapter currentTarget = ctx.Target.GetComponent<CombatEntityAdapter>();

            if (currentTarget != null && currentTarget.IsAlive)
                return NodeState.SUCCESS;

            ctx.Target = null;
        }

        // Search for a new target.
        ICombatEntity target = TargetingSystem.FindNearestEnemy(ctx.Combat, ctx.Targeting);

        if (target == null)
            return NodeState.FAILURE;

        ctx.Target = target.CombatTransform;

        //Debug.Log($"{name}: Behaviour Tree acquired {ctx.Target.name}");

        return NodeState.SUCCESS;
    }

    private bool IsTargetValid(EnemyContext ctx)
    {
        if (ctx.Target == null || ctx.Combat == null || ctx.Targeting == null)
            return false;

        CombatEntityAdapter target = ctx.Target.GetComponent<CombatEntityAdapter>();

        if (target == null)
            return false;

        return TargetingSystem.IsValidTarget(ctx.Combat, target, ctx.Targeting);
    }

    private bool IsDead(EnemyContext ctx)
    {
        return ctx.Character == null || ctx.Character.IsDead;
    }

    private Node BuildBehaviourTree()
    {
        return new Selector(new System.Collections.Generic.List<Node>
        {
          BuildDeathBranch(),
          BuildStunBranch(),
          BuildReturnBranch(),
          BuildTargetBranch()
        });
    }

    private Node BuildDeathBranch()
    {
        return new Sequence(new System.Collections.Generic.List<Node>
        {
          new ConditionNode(Context, IsDead),
          new ActionNode(Context, StopAI)
        });
    }

    private Node BuildStunBranch()
    {
        return new Sequence(new System.Collections.Generic.List<Node>
        {
          new ConditionNode(Context, IsStunned),
          new ActionNode(Context, StopAI)
        });
    }

    private Node BuildTargetBranch()
    {
        return new Sequence(new System.Collections.Generic.List<Node>
        {
          new ActionNode(Context, FindTarget),
          new ConditionNode(Context, HasTarget),
          new ConditionNode(Context, IsTargetValid),
          BuildCombatBranch()
        });
    }

    private Node BuildCombatBranch()
    {
        return new Selector(new System.Collections.Generic.List<Node>
        {
          BuildMeleeBranch(),
          BuildRangedBranch(),
          BuildChaseBranch()
        });
    }

    private Node BuildMeleeBranch()
    {
        return new Sequence(new System.Collections.Generic.List<Node>
        {
          new ConditionNode(Context, IsTargetInMeleeRange),
          new ActionNode(Context, PerformMeleeAttack)
        });
    }

    private Node BuildRangedBranch()
    {
        return new Sequence(new System.Collections.Generic.List<Node>
        {
          new ConditionNode(Context, IsTargetInRangedRange),
          new ActionNode(Context, PerformRangedAttack)
        });
    }

    private Node BuildChaseBranch()
    {
        return new ActionNode(Context, ChaseTarget);
    }

    private Node BuildReturnBranch()
    {
        return new Sequence(new System.Collections.Generic.List<Node>
        {
          new ConditionNode(Context, IsOutsideLeash),
          new ActionNode(Context, ReturnHome)
        });
    }

    private bool IsTargetInMeleeRange(EnemyContext ctx)
    {
        if (ctx.Target == null)
            return false;

        float distance = Vector3.Distance(transform.position, ctx.Target.position);
        return ctx.MeleeAttack != null && distance <= ctx.AttackRange;
    }

    private NodeState PerformMeleeAttack(EnemyContext ctx)
    {
        if (ctx.Target == null || ctx.Combat == null || ctx.MeleeAttack == null || ctx.Enemy == null)
            return NodeState.FAILURE;

        if (Time.time < nextMeleeAttackTime)
            return NodeState.RUNNING;

        CombatEntityAdapter target = ctx.Target.GetComponent<CombatEntityAdapter>();

        if (target == null || !target.IsAlive)
            return NodeState.FAILURE;

        ctx.Movement?.StopMovement();
        FaceTarget(ctx);

        MeleeManager meleeManager = ctx.MeleeManager;

        if (meleeManager == null)
            return NodeState.FAILURE;

        meleeManager.TryAttack(ctx.Combat, target, ctx.MeleeAttack);

        nextMeleeAttackTime = Time.time + ctx.AttackCheckInterval;

        return NodeState.SUCCESS;
    }

    private bool IsTargetInRangedRange(EnemyContext ctx)
    {
        if (ctx.Target == null)
            return false;

        float distance = Vector3.Distance(transform.position, ctx.Target.position);

        return ctx.RangedAttack != null && distance <= ctx.RangedAttackRange;
    }

    private NodeState PerformRangedAttack(EnemyContext ctx)
    {
        if (ctx.Target == null || ctx.Combat == null || ctx.RangedAttack == null || ctx.Enemy == null)
            return NodeState.FAILURE;

        if (Time.time < nextRangedAttackTime)
            return NodeState.RUNNING;

        CombatEntityAdapter target = ctx.Target.GetComponent<CombatEntityAdapter>();

        if (target == null || !target.IsAlive)
            return NodeState.FAILURE;

        ctx.Movement?.StopMovement();
        FaceTarget(ctx);

        RangedAttackManager rangedManager = ctx.RangedAttackManager;

        if (rangedManager == null)
            return NodeState.FAILURE;

        Transform spawnPoint = ctx.Enemy.ProjectileSpawnPoint;

        if (spawnPoint == null)
            return NodeState.FAILURE;

        bool fired = rangedManager.TryFire(ctx.Combat, target, ctx.RangedAttack, spawnPoint.position);

        if (!fired)
            return NodeState.FAILURE;

        nextRangedAttackTime = Time.time + ctx.AttackCheckInterval;

        return NodeState.SUCCESS;
    }

    private bool IsStunned(EnemyContext ctx)
    {
        return ctx.StatusEffects != null && ctx.StatusEffects.IsStunned();
    }

    private NodeState ChaseTarget(EnemyContext ctx)
    {
        if (ctx.Target == null || ctx.Movement == null)
            return NodeState.FAILURE;

        CombatEntityAdapter target = ctx.Target.GetComponent<CombatEntityAdapter>();

        if (target == null || !target.IsAlive)
        {
            ctx.Target = null;
            ctx.Movement.StopMovement();
            return NodeState.FAILURE;
        }

        Vector3 direction = ctx.Target.position - ctx.Enemy.transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            ctx.Movement.SetMovement(direction.normalized, ctx.ChaseSpeed);

        return NodeState.RUNNING;
    }

    private bool IsOutsideLeash(EnemyContext ctx)
    {
        if (ctx.Enemy == null)
            return false;

        if (ctx.IsReturningHome)
            return true;

        float distance = Vector3.Distance(
            ctx.Enemy.transform.position,
            ctx.Enemy.HomePosition
        );

        return distance >= ctx.LeashDistance;
    }

    private NodeState ReturnHome(EnemyContext ctx)
    {
        if (ctx.Enemy == null || ctx.Movement == null)
            return NodeState.FAILURE;

        // Discard the current target.
        ctx.Target = null;

        ctx.IsReturningHome = true;

        Vector3 direction = ctx.Enemy.HomePosition - ctx.Enemy.transform.position;

        direction.y = 0f;

        // Check whether the enemy has reached home.
        if (direction.sqrMagnitude < 0.25f)
        {
            ctx.Movement.StopMovement();
            ctx.IsReturningHome = false;
            return NodeState.SUCCESS;
        }

        ctx.Movement.SetMovement(direction.normalized, ctx.ReturnSpeed);
        return NodeState.RUNNING;
    }

    private bool HasTarget(EnemyContext ctx)
    {
        if (ctx.Target == null)
            return false;

        CombatEntityAdapter target = ctx.Target.GetComponent<CombatEntityAdapter>();

        return target != null && target.IsAlive;
    }

    private void FaceTarget(EnemyContext ctx)
    {
        if (ctx.Target == null || ctx.Enemy == null)
            return;

        Vector3 direction =
            ctx.Target.position - ctx.Enemy.transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            ctx.Enemy.transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    private NodeState StopAI(EnemyContext ctx)
    {
        ctx.Movement?.StopMovement();
        return NodeState.SUCCESS;
    }
}
