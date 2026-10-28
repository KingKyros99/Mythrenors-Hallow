using System.Collections.Generic;
using UnityEngine;

// MYTHRENOR'S HALLOW - MELEE MANAGER
//
// PURPOSE:
// Handles reusable melee attack execution,
// targeting validation, and attack cooldowns.
//
// TODO (JABARI INTEGRATION):
// Connect attack requests to Jabari's Player
// and Enemy controllers through ICombatEntity.
//
// NOTE:
// This system does not handle player input,
// animations, or character movement.

public class MeleeManager : MonoBehaviour
{
    // PURPOSE:
    // Stores the remaining cooldown for each
    // melee attack used by this manager instance.
    //
    // NOTE:
    // Cooldowns are not stored on ScriptableObjects,
    // since multiple Entities may share attack data.
    private Dictionary<MeleeAttackData, float> nextAttackTimes = new Dictionary<MeleeAttackData, float>();

    // PURPOSE:
    // Checks whether an attack can currently be used.
    public bool CanAttack(ICombatEntity attacker, MeleeAttackData attackData)
    {
        if (attacker == null || attackData == null)
        {
            return false;
        }

        if (!attacker.IsAlive || attacker.CombatTransform == null)
        {
            return false;
        }

        if (attackData.targetingData == null)
        {
            return false;
        }

        if (nextAttackTimes.TryGetValue(attackData, out float nextAttackTime))
        {
            if (Time.time < nextAttackTime)
            {
                return false;
            }
        }

        return true;
    }

    // PURPOSE:
    // Attempts a melee attack against a target.
    //
    // TODO (JABARI INTEGRATION):
    // Jabari's Entity will provide the attacker
    // and target references during combat.
    public bool TryAttack(ICombatEntity attacker, ICombatEntity target, MeleeAttackData attackData)
    {
        if (!CanAttack(attacker, attackData))
        {
            return false;
        }

        // Validate the target before attacking.
        if (target == null || !TargetingSystem.IsValidTarget(attacker, target, attackData.targetingData))
        {
            return false;
        }

        // Request damage through the shared
        // CombatManager instead of changing
        // Entity health directly.
        bool attackSuccessful = CombatManager.TryDealDamage(attacker, target, attackData.damage);

        if (!attackSuccessful)
        {
            return false;
        }

        // Begin the cooldown only after a
        // successful damage request.
        nextAttackTimes[attackData] = Time.time + Mathf.Max(0f, attackData.cooldown);

        return true;
    }

    // PURPOSE:
    // Attempts to attack the nearest valid enemy.
    //
    // TODO (JABARI INTEGRATION):
    // Useful for simple enemy AI, but player
    // targeting may use camera or input selection.
    public bool TryAttackNearest(ICombatEntity attacker, MeleeAttackData attackData)
    {
        if (!CanAttack(attacker, attackData))
        {
            return false;
        }

        ICombatEntity target = TargetingSystem.FindNearestEnemy(attacker, attackData.targetingData);

        if (target == null)
        {
            return false;
        }

        return TryAttack(attacker, target, attackData);
    }

    // PURPOSE:
    // Returns the remaining cooldown time
    // for a configured melee attack.
    public float GetRemainingCooldown(MeleeAttackData attackData)
    {
        if (attackData == null)
        {
            return 0f;
        }

        if (!nextAttackTimes.TryGetValue(attackData, out float nextAttackTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, nextAttackTime - Time.time);
    }

    // PURPOSE:
    // Clears the cooldown for one attack.
    // Useful when an Entity respawns or resets.
    public void ResetCooldown(MeleeAttackData attackData)
    {
        if (attackData == null)
        {
            return;
        }

        nextAttackTimes.Remove(attackData);
    }
}
