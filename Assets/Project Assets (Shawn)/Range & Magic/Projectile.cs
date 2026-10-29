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
    public void Initialize(ICombatEntity attacker, ProjectileData data, DamageData damage, Vector3 travelDirection)
    {
        if (attacker == null || data == null || travelDirection.sqrMagnitude <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        owner = attacker;

        projectileData = data;

        damageData = damage;

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

        if (hit.transform == transform || hit.transform.IsChildOf(transform))
        {
            return true;
        }

        if (owner != null && owner.CombatTransform != null)
        {
            Transform ownerTransform = owner.CombatTransform;

            if (hit.transform == ownerTransform || hit.transform.IsChildOf(ownerTransform))
            {
                return true;
            }
        }

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

        MonoBehaviour[] components = hit.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component is ICombatEntity target)
            {
                // CombatManager handles team checks,
                // damage calculation, and damage requests.
                CombatManager.TryDealDamage(owner, target, damageData);

                break;
            }
        }

        // TODO (DESIGN):
        // Add impact VFX, sound, and status effects.

        Destroy(gameObject);
    }
}
