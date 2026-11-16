using UnityEngine;

public class EnemyContext
{
    // Main enemy reference
    public Enemy Enemy { get; private set; }

    // Character information
    public CharacterBase Character { get; private set; }

    // Movement
    public EnemyController Movement { get; private set; }

    // Combat
    public CombatEntityAdapter Combat { get; private set; }

    public MeleeManager MeleeManager { get; private set; }

    public RangedAttackManager RangedAttackManager { get; private set; }

    // Status effects
    public StatusEffectManager StatusEffects { get; private set; }

    // Targeting
    public TargetingData Targeting { get; private set; }
    public float AttackCheckInterval { get; private set; }
    public MeleeAttackData MeleeAttack { get; private set; }
    public RangedAttackData RangedAttack { get; private set; }

    //Range
    public float AttackRange { get; private set; }
    public float RangedAttackRange { get; private set; }
    public float LeashDistance { get; private set; }

    public float PatrolSpeed { get; private set; }
    public float ChaseSpeed { get; private set; }
    public float ReturnSpeed { get; private set; }
    public bool IsReturningHome { get; set; }

    // Target shared between AI nodes
    public Transform Target { get; set; }

    public EnemyContext(Enemy enemy)
    {
        Enemy = enemy;

        if (enemy == null)
            return;

        Character = enemy.GetComponent<CharacterBase>();
        Movement = enemy.GetComponent<EnemyController>();
        Combat = enemy.GetComponent<CombatEntityAdapter>();
        StatusEffects = enemy.GetComponent<StatusEffectManager>();

        MeleeManager = enemy.MeleeManager;
        RangedAttackManager = enemy.RangedAttackManager;

        Targeting = enemy.Targeting;
        AttackCheckInterval = enemy.AttackCheckInterval;
        MeleeAttack = enemy.MeleeAttack;
        RangedAttack = enemy.RangedAttack;

        AttackRange = enemy.AttackRange;
        RangedAttackRange = enemy.RangedAttackRange;
        LeashDistance = enemy.LeashDistance;

        PatrolSpeed = enemy.PatrolSpeed;
        ChaseSpeed = enemy.ChaseSpeed;
        ReturnSpeed = enemy.ReturnSpeed;
    }
}
