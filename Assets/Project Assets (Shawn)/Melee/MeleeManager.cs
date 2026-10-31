using System;
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

        if (!StatusEffectManager.CanEntityAct(attacker))
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
    // Attempts a melee attack and starts its cooldown
    // once a valid attack has been executed.
    public bool TryAttack(ICombatEntity attacker, ICombatEntity target, MeleeAttackData attackData)
    {
        if (!CanAttack(attacker, attackData))
        {
            return false;
        }

        TargetingData targeting = attackData.targetingData;

        ICombatEntity validTarget = null;

        // Use an explicitly selected target when valid.
        if (target != null)
        {
            if (TargetingSystem.IsTargetInMeleeArc(attacker, target, targeting))
            {
                validTarget = target;
            }
        }

        else
        {
            // Automatically find an enemy inside the melee arc.
            validTarget = TargetingSystem.FindEnemyInMeleeArc(attacker, targeting);
        }

        // Attacks requiring a target cannot execute on a miss.
        if (attackData.requiresTarget && validTarget == null)
        {
            return false;
        }

        // A valid swing starts cooldown even if it misses.
        nextAttackTimes[attackData] = Time.time + Mathf.Max(0f, attackData.cooldown);

        // Play the melee slash once the swing is executed.
        PlaySwingVfx(attacker, attackData);

        if (validTarget == null)
        {
            Debug.Log($"{attacker.CombatTransform.name} swung but missed.");

            return true;
        }

        // Resolve the attack and retrieve its actual outcome.
        AttackOutcome outcome = CombatManager.ResolveAttack(attacker, validTarget, attackData.damage);

        // Report the outcome.
        switch (outcome)
        {
            case AttackOutcome.HIT:
                Debug.Log($"{attacker.CombatTransform.name} hit " + $"{validTarget.CombatTransform.name}!");
                break;
            case AttackOutcome.BLOCKED:
                Debug.Log($"{validTarget.CombatTransform.name} blocked the attack!");
                break;
            case AttackOutcome.DODGED:
                Debug.Log($"{validTarget.CombatTransform.name} dodged the attack!");
                break;
            case AttackOutcome.KILLED:
                Debug.Log($"{validTarget.CombatTransform.name} was defeated!");
                break;
            case AttackOutcome.MISSED:
                Debug.Log("Attack missed.");
                break;
            case AttackOutcome.INVALID:
                Debug.Log("Attack could not be resolved.");
                break;
        }

        // A blocked attack still connects, but a dodge does not.
        bool appliesStatus = outcome == AttackOutcome.HIT || outcome == AttackOutcome.BLOCKED;

        if (appliesStatus && attackData.statusEffect != null && validTarget.IsAlive)
        {
            StatusEffectManager statusManager = validTarget.CombatTransform.GetComponent<StatusEffectManager>();

            // Apply the configured status effect, if one exists.
            if (attackData.statusEffect != null)
            {
                StatusEffectManager statusManager = validTarget.CombatTransform.GetComponent<StatusEffectManager>();

                if (statusManager != null)
                {
                    statusManager.ApplyEffect(attackData.statusEffect);
                }
            }
        }
        return true;
    }

    // PURPOSE:
    // Spawns a configurable visual effect when
    // an Entity performs a melee swing.
    private void PlaySwingVfx(ICombatEntity attacker, MeleeAttackData attackData)
    {
        if (attacker == null || attackData == null || attacker.CombatTransform == null || attackData.swingVfxPrefab == null)
        {
            return;
        }

        Transform attackerTransform = attacker.CombatTransform;

        // Position the slash relative to the attacker.
        Vector3 spawnPosition = attackerTransform.TransformPoint(attackData.swingVfxOffset);

        // Face the same direction as the attacker.
        Quaternion spawnRotation = attackerTransform.rotation * Quaternion.Euler(attackData.swingVfxRotationOffset);

        // Spawn the effect separately from the character.
        GameObject effect = Instantiate(attackData.swingVfxPrefab, spawnPosition, spawnRotation);

        // Remove the effect after it finishes.
        Destroy(effect, Mathf.Max(0.1f, attackData.swingVfxLifetime));
    }

    // PURPOSE:
    // Spawns an impact effect on an Entity
    // when a melee attack successfully deals damage.
    private void PlayHitVfx(ICombatEntity attacker, ICombatEntity target, MeleeAttackData attackData)
    {
        if (attacker == null || target == null || attackData == null || attacker.CombatTransform == null || target.CombatTransform == null ||
            attackData.hitVfxPrefab == null)
        {
            return;
        }

        // Find the target's collider.
        Collider targetCollider = target.CombatTransform.GetComponentInChildren<Collider>();

        // Default to approximately chest height.
        Vector3 hitPosition = target.CombatTransform.position + Vector3.up;

        // Prefer the collider surface facing the attacker.
        if (targetCollider != null)
        {
            Vector3 attackOrigin = attacker.CombatTransform.position + Vector3.up;

            hitPosition = targetCollider.ClosestPoint(attackOrigin);
        }

        // Face the effect in the attacker's direction.
        Quaternion hitRotation = attacker.CombatTransform.rotation * Quaternion.Euler(attackData.hitVfxRotationOffset);

        GameObject effect = Instantiate(attackData.hitVfxPrefab, hitPosition, hitRotation);

        Destroy(effect, Mathf.Max(0.1f, attackData.hitVfxLifetime));
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

        ICombatEntity target = TargetingSystem.FindEnemyInMeleeArc(attacker, attackData.targetingData);

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
