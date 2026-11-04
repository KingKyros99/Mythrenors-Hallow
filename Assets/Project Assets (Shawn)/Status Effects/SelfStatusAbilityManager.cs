using System.Collections.Generic;
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
}
