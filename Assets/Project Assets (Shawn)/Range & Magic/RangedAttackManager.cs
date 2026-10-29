
using System.Collections.Generic;
using UnityEngine;

// MYTHRENOR'S HALLOW - RANGED ATTACK MANAGER
//
// PURPOSE:
// Handles ranged weapon attack requests,
// target validation, and attack cooldowns.
//
// TODO (JABARI INTEGRATION):
// Connect weapon attacks to Jabari's Player
// and Enemy controllers.
//
// TODO (JABARI INTEGRATION):
// Add ammunition validation and consumption
// when the inventory system is available.
//
// NOTE:
// Projectile movement and collisions are
// handled by the shared Projectile system.

public class RangedAttackManager : MonoBehaviour
{
    // Manager References
    [SerializeField] private ProjectileManager projectileManager;

    // PURPOSE:
    // Tracks cooldowns independently for each
    // configured attack on this manager instance.
    private Dictionary<RangedAttackData, float> nextAttackTimes = new Dictionary<RangedAttackData, float>();

    // PURPOSE:
    // Checks whether a ranged attack is available.
    public bool CanAttack(ICombatEntity attacker, RangedAttackData attackData)
    {
        if (attacker == null || attackData == null)
        {
            return false;
        }

        if (!attacker.IsAlive || attacker.CombatTransform == null)
        {
            return false;
        }

        if (projectileManager == null || attackData.projectileData == null || attackData.targetingData == null)
        {
            return false;
        }

        ProjectileData projectile = attackData.projectileData;

        if (projectile.projectilePrefab == null || projectile.speed <= 0f || projectile.maxRange <= 0f || projectile.lifetime <= 0f)
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

        // TODO (JABARI INTEGRATION):
        // Check ammunition before allowing attacks.
        return true;
    }

    // PURPOSE:
    // Fires a configured ranged weapon at a target.
    //
    // NOTE:
    // Spawn position comes from the caller so
    // weapons can have different firing points.
    public bool TryFire(ICombatEntity attacker, ICombatEntity target, RangedAttackData attackData, Vector3 spawnPosition)
    {
        if (!CanAttack(attacker, attackData))
        {
            return false;
        }

        if (!TargetingSystem.IsValidTarget(attacker, target, attackData.targetingData))
        {
            return false;
        }

        // Aim toward the target using the requested
        // spawn position as the firing point.
        Vector3 direction = target.CombatTransform.position - spawnPosition;

        if (direction.sqrMagnitude <= 0f)
        {
            return false;
        }

        // Spawn the projectile through our shared manager.
        bool successful = projectileManager.TrySpawnProjectile(
            attacker,
            attackData.projectileData,
            attackData.damage,
            spawnPosition,
            direction
        );

        if (!successful)
        {
            return false;
        }

        // Start the attack cooldown after spawning.
        nextAttackTimes[attackData] = Time.time + Mathf.Max(0f, attackData.cooldown);

        return true;
    }

    // PURPOSE:
    // Returns the remaining ranged attack cooldown.
    public float GetRemainingCooldown(RangedAttackData attackData)
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
    // Resets the cooldown for a ranged attack.
    public void ResetCooldown(RangedAttackData attackData)
    {
        if (attackData == null)
        {
            return;
        }

        nextAttackTimes.Remove(attackData);
    }
}
