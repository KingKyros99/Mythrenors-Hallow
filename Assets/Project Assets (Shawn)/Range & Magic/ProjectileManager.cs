using UnityEngine;

// MYTHRENOR'S HALLOW - PROJECTILE MANAGER
//
// PURPOSE:
// Creates and initializes projectiles used
// by ranged weapons and magic attacks.
//
// TODO (JABARI INTEGRATION):
// Jabari's Entity or equipment system will
// provide the attacker and spawn position.
//
// NOTE:
// This manager does not control ammo,
// mana consumption, or attack cooldowns.

public class ProjectileManager : MonoBehaviour
{
    // PURPOSE:
    // Spawns a projectile using its configuration,
    // attacker information, and travel direction.
    public bool TrySpawnProjectile(ICombatEntity attacker, ProjectileData projectileData, DamageData damageData, Vector3 spawnPosition, Vector3 direction)
    {
        if (attacker == null || !attacker.IsAlive || attacker.CombatTransform == null)
        {
            return false;
        }

        if (projectileData == null || projectileData.projectilePrefab == null)
        {
            return false;
        }

        if (direction.sqrMagnitude <= 0f)
        {
            return false;
        }

        if (projectileData.speed <= 0f || projectileData.maxRange <= 0f || projectileData.lifetime <= 0f)
        {
            return false;
        }

        Vector3 normalizedDirection = direction.normalized;

        Quaternion rotation = Quaternion.LookRotation(normalizedDirection);

        // Create a separate projectile instance.
        Projectile projectile = Instantiate(projectileData.projectilePrefab, spawnPosition, rotation);

        // Supply the projectile with the data needed
        // to operate independently after spawning.
        projectile.Initialize(attacker, projectileData, damageData, normalizedDirection);

        return true;
    }
}
