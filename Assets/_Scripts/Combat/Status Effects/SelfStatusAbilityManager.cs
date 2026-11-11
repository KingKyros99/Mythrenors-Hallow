using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

// MYTHRENOR'S HALLOW - SELF STATUS ABILITY MANAGER
//
// PURPOSE:
// Activates beneficial status abilities for
// both player and enemy combat Entities.
//
// TODO (JABARI INTEGRATION):
// Allow enemy AI to request abilities.
//
// NOTE:
// Each Entity maintains independent cooldowns.

[RequireComponent(typeof(StatusEffectManager))]
public class SelfStatusAbilityManager : MonoBehaviour
{
    // Entity References
    [SerializeField] private MonoBehaviour entityComponent;
    [SerializeField] private StatusEffectManager statusEffectManager;

    private ICombatEntity combatEntity;

    // Runtime cooldowns remain separate from shared assets.
    private readonly Dictionary<SelfStatusAbilityData, float> nextUseTimes = new Dictionary<SelfStatusAbilityData, float>();

    // PURPOSE:
    // Configurable usuage limits for each self-ability.
    // These values can be changed in Unity's Inspector.
    // Self-Ability Usage Limits
    [SerializeField, Min(0)] private int maxAttackBoostUses = 2;
    [SerializeField, Min(0)] private int maxDefenseBoostUses = 2;
    [SerializeField, Min(0)] private int maxRegenerationUses = 5;

    // PURPOSE:
    // Tracks remaining ability uses during gameplay.
    // Each character has its own counters.
    private int remainingAttackBoostUses;
    private int remainingDefenseBoostUses;
    private int remainingRegenerationUses;

    // PURPOSE:
    // Retrieves the character and status effect manager.
    private void Awake()
    {
        if (entityComponent == null)
        {
            entityComponent = GetComponent<CombatEntityAdapter>();
        }

        if (statusEffectManager == null)
        {
            statusEffectManager = GetComponent<StatusEffectManager>();
        }

        combatEntity = entityComponent as ICombatEntity;

        ResetAbilityUses();
    }

    // PURPOSE:
    // Restores all self-ability uses.
    // Can also be called when starting a new encounter.
    private void ResetAbilityUses()
    {
        remainingAttackBoostUses = maxAttackBoostUses;

        remainingDefenseBoostUses = maxDefenseBoostUses;

        remainingRegenerationUses = maxRegenerationUses;

        nextUseTimes.Clear();
    }

    // PURPOSE:
    // Activates a configured beneficial status ability.
    public bool TryActivate(SelfStatusAbilityData ability)
    {
        if (ability == null || ability.statusEffect == null || combatEntity == null || statusEffectManager == null || !statusEffectManager.isActiveAndEnabled)
        {
            return false;
        }

        // Verify the character is alive and able to act.
        if (!StatusEffectManager.CanEntityAct(combatEntity))
        {
            return false;
        }

        // Only activate abilities belonging to this Entity.
        if (combatEntity.CombatTransform != transform)
        {
            return false;
        }

        // Self abilities cannot apply harmful effects.
        StatusEffectType effectType = ability.statusEffect.effectType;

        if (effectType != StatusEffectType.Regeneration && effectType != StatusEffectType.AttackBuff && effectType != StatusEffectType.DefenseBuff)
        {
            return false;
        }

        // Reject invalid mana costs.
        if (float.IsNaN(ability.manaCost) || float.IsInfinity(ability.manaCost) || ability.manaCost < 0f)
        {
            return false;
        }

        // PURPOSE:
        // Reject self-abilities that have no uses left.
        // This happens before any mana is consumed.
        if (GetRemainingUses(ability) <= 0)
        {
            Debug.Log($"{name}: No remaining uses for {ability.abilityName}.");

            return false;
        }

        // Check the ability's cooldown.
        if (nextUseTimes.TryGetValue(ability, out float nextUseTime))
        {
            if (Time.time < nextUseTime)
            {
                return false;
            }
        }

        // Spend mana using Jabari's character system.
        if (!combatEntity.TrySpendMana(ability.manaCost))
        {
            return false;
        }

        // Apply the status effect to this character.
        bool applied = statusEffectManager.ApplyEffect(ability.statusEffect);

        if (!applied)
        {
            combatEntity.RefundMana(ability.manaCost);

            return false;
        }

        // PURPOSE:
        // Consume one use only after the status effect
        // has been activated successfully.
        switch (effectType)
        {
            case StatusEffectType.AttackBuff:

                remainingAttackBoostUses--;

                break;

            case StatusEffectType.DefenseBuff:

                remainingDefenseBoostUses--;

                break;

            case StatusEffectType.Regeneration:

                remainingRegenerationUses--;

                break;
        }

        // Start cooldown only after successful activation.
        nextUseTimes[ability] = Time.time + Mathf.Max(0f, ability.cooldown);

        Debug.Log($"{name} activated {ability.abilityName}.");

        return true;
    }

    // PURPOSE:
    // Returns the remaining cooldown for an ability.
    public float GetRemainingCooldown(SelfStatusAbilityData ability)
    {
        if (ability == null || !nextUseTimes.TryGetValue(ability, out float nextUseTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, nextUseTime - Time.time);
    }

    // PURPOSE:
    // Returns how many uses remain for a self-ability.
    // Can be used by the combat hotbar later.
    public int GetRemainingUses(SelfStatusAbilityData ability)
    {
        if (ability == null || ability.statusEffect == null)
        {
            return 0;
        }

        switch (ability.statusEffect.effectType)
        {
            case StatusEffectType.AttackBuff:

                return remainingAttackBoostUses;

            case StatusEffectType.DefenseBuff:

                return remainingDefenseBoostUses;

            case StatusEffectType.Regeneration:

                return remainingRegenerationUses;

            default:

                return 0;
        }
    }

    // PURPOSE:
    // Returns the maximum number of uses
    // configured for a self-ability.
    // Used by the Combat Hotbar.
    public int GetMaxUses(SelfStatusAbilityData ability)
    {
        if (ability == null || ability.statusEffect == null)
        {
            return 0;
        }

        switch (ability.statusEffect.effectType)
        {
            case StatusEffectType.AttackBuff:

                return maxAttackBoostUses;

            case StatusEffectType.DefenseBuff:

                return maxDefenseBoostUses;

            case StatusEffectType.Regeneration:

                return maxRegenerationUses;

            default:

                return 0;
        }
    }
}
