using UnityEngine;

// MYTHRENOR'S HALLOW - ENEMY ATTACK ANIMATOR
//
// PURPOSE:
// Plays an enemy's attack animation while
// its AI is in the Attacking state.
//
// NOTE:
// Reusable for different melee enemies.
// Does not change combat damage or enemy AI.

public class EnemyAttackAnimator : MonoBehaviour
{
    // References
    [SerializeField] private Enemy enemy;
    [SerializeField] private Animator animator;

    // Animation Settings
    [SerializeField] private string attackTrigger = "Attack";

    [SerializeField, Min(0.1f)]
    private float attackAnimationInterval = 1.2f;

    private int attackTriggerHash;
    private float nextAttackAnimationTime;
    private bool wasAttacking;

    private void Awake()
    {
        // Find the active Enemy component.
        if (enemy == null || !enemy.isActiveAndEnabled)
        {
            foreach (Enemy candidate in GetComponents<Enemy>())
            {
                if (candidate.isActiveAndEnabled)
                {
                    enemy = candidate;

                    break;
                }
            }
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        attackTriggerHash = Animator.StringToHash(attackTrigger);
    }

    private void Update()
    {
        if (enemy == null || !enemy.isActiveAndEnabled || animator == null || !animator.isActiveAndEnabled)
        {
            return;
        }

        // Only animate attacks while the AI is attacking.
        bool isAttacking = enemy.CurrentState == EnemyState.Attacking;

        if (!isAttacking)
        {
            wasAttacking = false;

            return;
        }

        // Allow an immediate swing when entering attack range.
        if (!wasAttacking)
        {
            wasAttacking = true;

            nextAttackAnimationTime = Time.time;
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // Don't restart an attack or interrupt a hit reaction.
        if (state.IsName("Attack") || state.IsName("HitReaction") || animator.IsInTransition(0))
        {
            return;
        }

        // Avoid triggering the animation every frame.
        if (Time.time < nextAttackAnimationTime)
        {
            return;
        }

        animator.SetTrigger(attackTriggerHash);

        nextAttackAnimationTime = Time.time + attackAnimationInterval;
    }
}
