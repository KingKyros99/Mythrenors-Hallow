using UnityEngine;

// MYTHRENOR'S HALLOW - DAMAGE REACTION ANIMATOR
//
// PURPOSE:
// Plays a character's hit reaction animation whenever
// CharacterBase reports that damage was received.
//
// Reusable for enemies without modifying combat logic.

public class DamageReactionAnimator : MonoBehaviour
{
    // References
    [SerializeField] private CharacterBase characterBase;
    [SerializeField] private Animator animator;

    [SerializeField] private StatusEffectManager statusEffectManager;

    // Animation Settings
    [SerializeField] private string hitTrigger = "TakeHit";

    private int hitTriggerHash;

    private void Awake()
    {
        // Find references automatically if not assigned.
        if (characterBase == null)
        {
            characterBase = GetComponent<CharacterBase>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        if (statusEffectManager == null)
        {
            statusEffectManager = GetComponent<StatusEffectManager>();
        }

        hitTriggerHash = Animator.StringToHash(hitTrigger);
    }

    private void OnEnable()
    {
        // Listen for actual damage received.
        if (characterBase != null)
        {
            characterBase.OnDirectDamageTaken += HandleDamageTaken;
        }
    }

    private void OnDisable()
    {
        // Prevent duplicate or stale event subscriptions.
        if (characterBase != null)
        {
            characterBase.OnDirectDamageTaken -= HandleDamageTaken;
        }
    }

    private void HandleDamageTaken(int damage)
    {
        if (damage <= 0 || characterBase == null || characterBase.IsDead || animator == null || !animator.isActiveAndEnabled)
        {
            return;
        }

        // Ignore damage from Burn, Bleed, and Poison ticks.
        if (statusEffectManager != null && statusEffectManager.IsApplyingStatusDamage)
        {
            return;
        }

        // Play the reaction for direct attacks.
        animator.SetTrigger(hitTriggerHash);
    }
}
