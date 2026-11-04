using UnityEngine;

// MYTHRENOR'S HALLOW - PROJECTILE AIM UTILITY
//
// PURPOSE:
// Provides a shared aiming position for
// ranged weapons and projectile spells.
//
// NOTE:
// Uses the target's collider center rather
// than its potentially ground-level Transform.

public static class ProjectileAimUtility
{
    // PURPOSE:
    // Returns a target's center for projectile aiming.
    public static Vector3 GetAimPoint(ICombatEntity target)
    {
        if (target == null || target.CombatTransform == null)
        {
            return Vector3.zero;
        }

        Transform targetTransform = target.CombatTransform;

        // Prefer the main collider on the Entity.
        Collider mainCollider = targetTransform.GetComponent<Collider>();

        if (IsValidAimCollider(mainCollider))
        {
            return mainCollider.bounds.center;
        }

        // Support Entities whose colliders are on children.
        Collider[] childColliders = targetTransform.GetComponentsInChildren<Collider>();

        foreach (Collider collider in childColliders)
        {
            if (IsValidAimCollider(collider))
            {
                return collider.bounds.center;
            }
        }

        // Fallback when the Entity has no usable collider.
        return targetTransform.position;
    }

    // PURPOSE:
    // Prevents aiming at disabled or trigger colliders.
    private static bool IsValidAimCollider(Collider collider)
    {
        return collider != null && collider.enabled && collider.gameObject.activeInHierarchy && !collider.isTrigger;
    }
}
