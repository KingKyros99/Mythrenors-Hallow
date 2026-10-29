using System.Collections.Generic;
using UnityEngine;

// MYTHRENOR'S HALLOW - TARGETING SYSTEM
//
// PURPOSE:
// Provides reusable target validation and selection
// for melee, ranged weapons, spells, and enemy AI.
//
// TODO (JABARI INTEGRATION):
// Connect target selection requests to Jabari's
// Player and Enemy controllers.
//
// NOTE:
// This system does not control movement or cameras.

public static class TargetingSystem
{
    // PURPOSE:
    // Checks whether a target is valid for an attack.
    public static bool IsValidTarget(ICombatEntity user, ICombatEntity target, TargetingData data)
    {
        if (user == null || target == null || data == null)
        {
            return false;
        }

        if (!user.IsAlive || !target.IsAlive)
        {
            return false;
        }

        Transform origin = user.CombatTransform;

        Transform destination = target.CombatTransform;

        if (origin == null || destination == null)
        {
            return false;
        }

        bool isSelf = ReferenceEquals(user, target) || origin == destination;

        // Self targeting does not require a distance check.
        if (data.targetType == TargetType.Self)
        {
            return isSelf;
        }

        if (isSelf && !data.allowSelfTarget)
        {
            return false;
        }

        // TODO (JABARI INTEGRATION):
        // TeamId is provisional until the faction
        // system is finalized.
        bool sameTeam = user.TeamId == target.TeamId;

        switch (data.targetType)
        {
            case TargetType.SingleEnemy:

            case TargetType.NearestEnemy:

            case TargetType.AllEnemiesInRadius:

            case TargetType.Directional:
                if (sameTeam)
                {
                    return false;
                }

                break;

            case TargetType.SingleAlly:
                if (!sameTeam)
                {
                    return false;
                }

                break;
        }

        float distance = Vector3.Distance(origin.position, destination.position);

        if (distance > data.maxRange)
        {
            return false;
        }

        // Optional obstacle checking.
        if (data.requireLineOfSight)
        {
            Vector3 direction = destination.position - origin.position;

            if (Physics.Raycast(origin.position, direction.normalized, direction.magnitude, data.obstructionLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }
        }

        return true;
    }

    // PURPOSE:
    // Finds a combat Entity attached to a collider
    // or its parent GameObject.
    //
    // TODO (JABARI INTEGRATION):
    // Verify that the Entity or adapter implements
    // ICombatEntity on a MonoBehaviour component.
    private static ICombatEntity FindCombatEntity(Collider hit)
    {
        MonoBehaviour[] components = hit.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component is ICombatEntity entity)
            {
                return entity;
            }
        }

        return null;
    }

    // PURPOSE:
    // Finds the nearest valid enemy in detection range.
    public static ICombatEntity FindNearestEnemy(ICombatEntity user, TargetingData data)
    {
        if (user == null || data == null || user.CombatTransform == null)
        {
            return null;
        }

        Collider[] hits = Physics.OverlapSphere(user.CombatTransform.position, data.maxRange, data.targetLayers);

        ICombatEntity nearest = null;

        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            ICombatEntity candidate = FindCombatEntity(hit);

            if (!IsValidTarget(user, candidate, data))
            {
                continue;
            }

            float distance = Vector3.Distance(user.CombatTransform.position, candidate.CombatTransform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;

                nearest = candidate;
            }
        }

        return nearest;
    }

    // PURPOSE:
    // Checks whether a target is inside the
    // attacker's forward-facing melee arc.
    public static bool IsTargetInMeleeArc(ICombatEntity user, ICombatEntity target, TargetingData data)
    {
        if (!IsValidTarget(user, target, data))
        {
            return false;
        }

        Vector3 direction = target.CombatTransform.position - user.CombatTransform.position;

        Vector3 forward = user.CombatTransform.forward;

        // Ignore vertical differences for melee direction.
        direction.y = 0f;

        forward.y = 0f;

        if (direction.sqrMagnitude <= 0f || forward.sqrMagnitude <= 0f)
        {
            return false;
        }

        float angle = Vector3.Angle(forward, direction);

        return angle <= data.attackAngle * 0.5f;
    }

    // PURPOSE:
    // Finds the nearest valid enemy within
    // a forward-facing melee attack arc.
    public static ICombatEntity FindEnemyInMeleeArc(ICombatEntity user, TargetingData data)
    {
        if (user == null || data == null || !user.IsAlive || user.CombatTransform == null)
        {
            return null;
        }

        Transform origin = user.CombatTransform;

        // Find potential targets within attack range.
        Collider[] hits = Physics.OverlapSphere(origin.position, data.maxRange, data.targetLayers, QueryTriggerInteraction.Collide);

        ICombatEntity nearest = null;

        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            ICombatEntity candidate = FindCombatEntity(hit);

            // Verify range, team, line of sight
            // and thr forward-facing attack arc.
            if (!IsTargetInMeleeArc(user, candidate, data))
            {
                continue;
            }

            // Calculate distance to the valid target.
            float distance = Vector3.Distance(origin.position, candidate.CombatTransform.position);

            // Keep the nearest valid enemy
            if (distance < closestDistance)
            {
                closestDistance = distance;

                nearest = candidate;
            }
        }

        return nearest;
    }

    // PURPOSE:
    // Finds all valid combat Entities within an area.
    //
    // TODO (DESIGN):
    // Separate the ability's casting range from
    // the area effect's radius during integration.
    //
    // TODO (JABARI INTEGRATION):
    // Confirm how Entity positions and colliders
    // will be configured.
    public static List<ICombatEntity> FindEnemiesInRadius(ICombatEntity user, Vector3 center, TargetingData data)
    {
        List<ICombatEntity> targets = new List<ICombatEntity>();

        if (user == null || data == null || !user.IsAlive || user.CombatTransform == null)
        {
            return targets;
        }

        if (data.targetType != TargetType.AllEnemiesInRadius)
        {
            return targets;
        }

        float castDistance = Vector3.Distance(user.CombatTransform.position, center);

        if (castDistance > data.maxRange)
        {
            return targets;
        }

        // Find the colliders inside the area, not around the caster.
        Collider[] hits = Physics.OverlapSphere(center, data.radius, data.targetLayers);

        foreach (Collider hit in hits)
        {
            ICombatEntity candidate = FindCombatEntity(hit);

            if (candidate == null || !candidate.IsAlive || candidate.CombatTransform == null)
            {
                continue;
            }

            // Exclude the caster and friendly characters.
            if (candidate.CombatTransform == user.CombatTransform || candidate.TeamId == user.TeamId)
            {
                continue;
            }

            // A target must actually be inside the area.
            float targetDistance = Vector3.Distance(center, candidate.CombatTransform.position);

            if (targetDistance > data.radius)
            {
                continue;
            }

            // Optional obstacle validation from the area center
            if (data.requireLineOfSight)
            {
                Vector3 direction = candidate.CombatTransform.position - center;

                if (direction.sqrMagnitude > 0f && Physics.Raycast(center, direction.normalized, direction.magnitude, data.obstructionLayers, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }
            }

            // Do not include the same Entity more than once
            if (!targets.Contains(candidate))
            {
                targets.Add(candidate);
            }
        }

        return targets;
    }

    // PURPOSE:
    // Finds a combat Entity in the user's forward direction.
    //
    // TODO (JABARI INTEGRATION):
    // Confirm whether directional targeting uses
    // the Entity's forward direction, camera,
    // or weapon aiming position.
    //
    // TODO (DESIGN):
    // Account for obstacles and other colliders
    // that may block directional attacks.
    public static ICombatEntity FindDirectionalTarget(ICombatEntity user, TargetingData data)
    {
        if (user == null || data == null || user.CombatTransform == null)
        {
            return null;
        }

        Ray ray = new Ray(user.CombatTransform.position, user.CombatTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, data.maxRange, data.targetLayers, QueryTriggerInteraction.Collide))
        {
            ICombatEntity candidate = FindCombatEntity(hit.collider);

            if (IsValidTarget(user, candidate, data))
            {
                return candidate;
            }
        }

        return null;
    }
}
