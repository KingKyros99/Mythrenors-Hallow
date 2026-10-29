using System.Collections.Generic;
using UnityEngine;

// MYTHRENOR'S HALLOW - STATUS EFFECT MANAGER
//
// PURPOSE:
// Applies, updates, and removes status effects
// for both player and enemy combat Entities.
//
// TODO (FUTURE):
// Add stun, slow, buffs, and status resistance.
//
// NOTE:
// Each Entity owns a separate manager instance.

public class StatusEffectManager : MonoBehaviour
{
    // Entity Reference
    [SerializeField] private MonoBehaviour entityComponent;

    private ICombatEntity combatEntity;

    // PURPOSE:
    // Tracks the runtime state of a single effect.
    private class ActiveStatusEffect
    {
        public StatusEffectData data;
        public float remainingDuration;
        public float tickTimer;
        public int stacks;
    }

    private readonly List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

    // PURPOSE:
    // Retrieves the combat Entity attached to this character.
    private void Awake()
    {
        if (entityComponent == null)
        {
            entityComponent = GetComponent<CombatEntityAdapter>();
        }

        combatEntity = entityComponent as ICombatEntity;

        if (combatEntity == null)
        {
            Debug.LogWarning($"{name}: No valid combat Entity assigned.", this);
        }
    }

    // PURPOSE:
    // Updates active effect durations and periodic ticks.
    private void Update()
    {
        if (combatEntity == null)
        {
            return;
        }

        if (!combatEntity.IsAlive)
        {
            activeEffects.Clear();

            return;
        }

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveStatusEffect effect = activeEffects[i];

            float elapsed = Mathf.Min(Time.deltaTime, effect.remainingDuration);

            effect.remainingDuration -= elapsed;

            effect.tickTimer += elapsed;

            float interval = Mathf.Max(0.1f, effect.data.tickInterval);

            while (effect.tickTimer + 0.0001f >= interval && combatEntity.IsAlive)
            {
                effect.tickTimer -= interval;

                ApplyTick(effect);
            }

            if (!combatEntity.IsAlive)
            {
                activeEffects.Clear();

                return;
            }

            if (effect.remainingDuration <= 0f)
            {
                Debug.Log($"{name}: {effect.data.effectName} expired.");

                activeEffects.RemoveAt(i);
            }
        }
    }

    // PURPOSE:
    // Applies a new status effect or refreshes
    // an effect already active on the Entity.
    public bool ApplyEffect(StatusEffectData data)
    {
        if (combatEntity == null || !combatEntity.IsAlive || data == null)
        {
            return false;
        }

        // Only implemented effect types can be applied.
        if (data.effectType != StatusEffectType.Poison && data.effectType != StatusEffectType.Regeneration)
        {
            return false;
        }

        foreach (ActiveStatusEffect effect in activeEffects)
        {
            if (effect.data != data)
            {
                continue;
            }

            if (data.canStack)
            {
                effect.stacks = Mathf.Min(effect.stacks + 1, Mathf.Max(1, data.maxStacks));
            }

            // Reapplying refreshes duration and tick timing.
            effect.remainingDuration = Mathf.Max(0.1f, data.duration);

            effect.tickTimer = 0f;

            return true;
        }

        ActiveStatusEffect newEffect = new ActiveStatusEffect
        {
            data = data, remainingDuration = Mathf.Max(0.1f, data.duration), tickTimer = 0f, stacks = 1
        };

        activeEffects.Add(newEffect);

        Debug.Log($"{name}: {data.effectName} applied.");

        return true;
    }

    // PURPOSE:
    // Processes periodic damage or healing.
    private void ApplyTick(ActiveStatusEffect effect)
    {
        float amount = Mathf.Max(0f, effect.data.effectPower) * effect.stacks;

        if (amount <= 0f)
        {
            return;
        }

        switch (effect.data.effectType)
        {
            case StatusEffectType.Poison:

                combatEntity.ReceiveDamage(amount);

                break;

            case StatusEffectType.Regeneration:

                combatEntity.ReceiveHealing(amount);

                break;
        }

        Debug.Log($"{name}: {effect.data.effectName} ticked for {amount}.");
    }

    // PURPOSE:
    // Removes a specific effect from the Entity.
    public void RemoveEffect(StatusEffectData data)
    {
        activeEffects.RemoveAll(effect => effect.data == data);
    }

    // PURPOSE:
    // Checks whether an effect is currently active.
    public bool HasEffect(StatusEffectData data)
    {
        return activeEffects.Exists(effect => effect.data == data);
    }

    // PURPOSE:
    // Removes every active status effect.
    public void ClearEffects()
    {
        activeEffects.Clear();
    }
}
