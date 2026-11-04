using UnityEngine;

// MYTHRENOR'S HALLOW - RANGED ATTACK DATA
//
// PURPOSE:
// Stores configurable ranged weapon information
// including damage, targeting, and cooldowns.
//
// TODO (JABARI INTEGRATION):
// Connect ranged weapons to Jabari's Entity
// equipment and inventory systems.
//
// NOTE:
// Ammunition consumption will be integrated
// once the Entity inventory API is finalized.

[CreateAssetMenu(fileName = "NewRangedAttack", menuName = "Mythrenors Hallow/Combat/Ranged Attack")]
public class RangedAttackData : ScriptableObject
{
    // Attack Information
    public string attackName = "New Ranged Attack";

    [TextArea]
    public string description;

    // Damage Settings
    public DamageData damage;

    // Projectile Settings
    public ProjectileData projectileData;

    // Targeting Settings
    public TargetingData targetingData;

    // Attack Timing
    [Min(0f)]
    public float cooldown = 1f;

    // Ammunition Settings
    public bool usesAmmo = false;

    [Min(0)]
    public int ammoCost = 1;

    // Status Effect
    public StatusEffectData statusEffect;
}
