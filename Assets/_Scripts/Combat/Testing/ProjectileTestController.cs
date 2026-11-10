using UnityEngine;

// MYTHRENOR'S HALLOW - PROJECTILE TEST CONTROLLER
//
// PURPOSE:
// Tests projectile spawning, movement, and
// combat damage without Jabari's Entity system.
//
// NOTE:
// TESTING ONLY. Not intended for gameplay scenes.

public class ProjectileTestController : MonoBehaviour
{
    // Test References
    public MockCombatEntity attacker;
    public MockCombatEntity target;

    public ProjectileManager projectileManager;
    public ProjectileData projectileData;

    // Test Damage
    public float baseDamage = 8f;

    // Test Settings
    [Min(0.1f)]
    public float testInterval = 2f;

    private float nextTestTime;

    private void Update()
    {
        if (Time.time < nextTestTime)
        {
            return;
        }

        nextTestTime = Time.time + testInterval;

        if (attacker == null || target == null || projectileManager == null || projectileData == null)
        {
            return;
        }

        if (!attacker.IsAlive || !target.IsAlive)
        {
            return;
        }

        // Spawn slightly in front of the attacker
        // to keep the projectile outside its collider.
        Vector3 spawnPosition = attacker.transform.position + Vector3.up * 0.5f;

        // Aim toward the target's upper body.
        Vector3 targetPosition = target.transform.position + Vector3.up * 0.5f;

        Vector3 direction = (targetPosition - spawnPosition).normalized;

        DamageData damage = new DamageData(baseDamage, DamageType.Physical, false, false);

        bool successful = projectileManager.TrySpawnProjectile(attacker, projectileData, damage, spawnPosition, direction);

        if (successful)
        {
            Debug.Log("Test projectile spawned.");
        }
    }
}
