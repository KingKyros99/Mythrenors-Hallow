using UnityEngine;

// MYTHRENOR'S HALLOW - PROJECTILE DATA
//
// PURPOSE:
// Stores configurable projectile settings
// used by ranged weapons and projectile spells.
//
// TODO (JABARI INTEGRATION):
// Allow Jabari's Entities to reference projectile
// configurations through weapons and abilities.
//
// NOTE:
// Projectile settings are shared assets.
// Runtime projectile state is stored separately.

[CreateAssetMenu(fileName = "NewProjectileData", menuName = "Mythrenors Hallow/Combat/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    // Projectile Information
    public string projectileName = "New Projectile";

    // Projectile Prefab
    public Projectile projectilePrefab;

    // Movement Settings
    [Min(0.01f)]
    public float speed = 15f;

    [Min(0.1f)]
    public float maxRange = 25f;

    [Min(0.1f)]
    public float lifetime = 5f;

    // Collision Settings
    [Min(0.01f)]
    public float collisionRadius = 0.15f;

    public LayerMask collisionLayers = ~0;
}
