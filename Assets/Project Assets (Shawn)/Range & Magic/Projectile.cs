using Unity.Cinemachine;
using UnityEngine;

// MYTHRENOR'S HALLOW - PROJECTILE
//
// PURPOSE:
// Handles projectile movement, collision,
// damage requests, and projectile cleanup.
//
// TODO (JABARI INTEGRATION):
// Projectiles receive their attacker through
// ICombatEntity rather than Player or Enemy.
//
// NOTE:
// Projectile movement is currently straight-line.
// Gravity, homing, and status effects come later.

public class Projectile : MonoBehaviour
{
    private ICombatEntity owner;
    private ProjectileData projectileData;
    private DamageData damageData;
    private StatusEffectData statusEffect;
    private Vector3 direction;
    private float distanceTraveled;
    private float remainingLifetime;

    private bool initialized;
    private bool hasHit;

    // PURPOSE:
    // Detects objects already overlapping the
    // projectile when it first spawns.
    private bool CheckInitialOverlap()
    {
        if (!initialized || hasHit || projectileData == null)
        {
            return false;
        }

        Collider[] overlaps = Physics.OverlapSphere(transform.position, projectileData.collisionRadius, projectileData.collisionLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in overlaps)
        {
            if (ShouldIgnoreCollision(hit))
            {
                continue;
            }

            // Process the first valid collision.
            HandleImpact(hit);

            return true;
        }

        return false;
    }

    // PURPOSE:
    // Configures a projectile after spawning.
    public void Initialize(ICombatEntity attacker, ProjectileData data, DamageData damage, Vector3 travelDirection, StatusEffectData newStatusEffect = null)
    {
        if (attacker == null || data == null || travelDirection.sqrMagnitude <= 0f)
        {
            Destroy(gameObject);

            return;
        }

        owner = attacker;

        projectileData = data;

        damageData = damage;

        statusEffect = newStatusEffect;

        direction = travelDirection.normalized;

        remainingLifetime = data.lifetime;

        initialized = true;
    }

    // PURPOSE:
    // Moves the projectile and checks for collisions
    // during each physics update.
    private void FixedUpdate()
    {
        if (!initialized || hasHit)
        {
            return;
        }

        // Detect enemies already overlapping
        // the projectile's current position.
        if (CheckInitialOverlap())
        {
            return;
        }

        remainingLifetime -= Time.fixedDeltaTime;

        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);

            return;
        }

        float remainingRange = projectileData.maxRange - distanceTraveled;

        if (remainingRange <= 0f)
        {
            Destroy(gameObject);

            return;
        }

        float movementDistance = Mathf.Min(projectileData.speed * Time.fixedDeltaTime, remainingRange);

        // Check the path before moving so fast
        // projectiles can detect nearby colliders.
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, projectileData.collisionRadius, direction, movementDistance, projectileData.collisionLayers, QueryTriggerInteraction.Collide);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreCollision(hit.collider))
            {
                continue;
            }

            transform.position += direction * hit.distance;

            HandleImpact(hit.collider);

            return;
        }

        transform.position += direction * movementDistance;

        distanceTraveled += movementDistance;

        if (distanceTraveled >= projectileData.maxRange)
        {
            Destroy(gameObject);
        }
    }

    // PURPOSE:
    // Prevents the projectile from hitting its owner
    // or colliders belonging to the projectile itself.
    private bool ShouldIgnoreCollision(Collider hit)
    {
        if (hit == null)
        {
            return true;
        }

        // Ignore the projectile's own colliders.
        if (hit.transform == transform || hit.transform.IsChildOf(transform))
        {
            return true;
        }

        // Ignore colliders belonging to the attacker
        if (owner != null && owner.CombatTransform != null)
        {
            Transform ownerTransform = owner.CombatTransform;

            if (hit.transform == ownerTransform || hit.transform.IsChildOf(ownerTransform))
            {
                return true;
            }
        }

        if (hit.isTrigger)
        {
            MonoBehaviour[] components = hit.GetComponentsInParent<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                // Combat Entity triggers may represent hitboxes.
                if (component is ICombatEntity)
                {
                    return false;
                }
            }

            return true;
        }

        // All other solid collisions are processed.
        return false;
    }

    // PURPOSE:
    // Checks whether the hit object can receive
    // combat damage through ICombatEntity.
    private void HandleImpact(Collider hit)
    {
        if (hasHit)
        {
            return;
        }

        hasHit = true;

        // Search for an Entity attached to the hit object.
        ICombatEntity target = null;

        MonoBehaviour[] components = hit.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component is ICombatEntity entity)
            {
                target = entity;

                break;
            }
        }

        // Handle an environmental impact.
        if (target == null)
        {
            Debug.Log($"{name} hit environment: {hit.name}");

            SpawnImpactVfx(projectileData.environmentImpactVfx, hit);
        }

        else
        {
            // Apply damage through the shared combat system.
            bool damageApplied = CombatManager.TryDealDamage(owner, target, damageData);

            if (damageApplied)
            {
                Debug.Log($"{name} hit combat Entity: {target.CombatTransform.name}");

                // Apply optional status effect
                if (target.IsAlive && statusEffect != null)
                {
                    StatusEffectManager statusManager = target.CombatTransform.GetComponent<StatusEffectManager>();

                    if (statusManager != null)
                    {
                        statusManager.ApplyEffect(statusEffect);
                    }
                }

                SpawnImpactVfx(projectileData.combatImpactVfx, hit);
            }

            else
            {
                Debug.Log($"{name} hit {hit.name}, but no damage was applied");

                // TODO (CORE FUNCTIONS INTEGRATION):
                // Distinguish blocked, dodged and invalid hits.
            }
        }

        // Remove the projectile after processing impact.
        Destroy(gameObject);
    }

    // PURPOSE:
    // Spawns the configured effect at the collision point.
    private void SpawnImpactVfx(GameObject effectPrefab, Collider hit)
    {
        if (effectPrefab == null || hit == null || projectileData == null)
        {
            return;
        }

        // Find the point on the collider nearest
        // to the projectile's current position.
        Vector3 impactPosition = hit.ClosestPoint(transform.position);

        // Spawn the effect independently of the projectile.
        GameObject effect = Instantiate(effectPrefab, impactPosition, Quaternion.identity);

        // Clean up the effect after its configured lifetime.
        Destroy(effect, projectileData.impactVfxLifetime);
    }
}
