using UnityEngine;

// MYTHRENOR'S HALLOW - SPELL DATA
//
// PURPOSE:
// Stores configurable magic attack information
// including damage, mana costs, and cooldowns.
//
// TODO (JABARI INTEGRATION):
// Connect spells to Jabari's Entity abilities
// and mana/resource management systems.
//
// NOTE:
// This initial version supports projectile
// damage spells. Healing and status effects
// will be integrated in later milestones.

[CreateAssetMenu(fileName = "NewSpell", menuName = "Mythrenors Hallow/Combat/Spell")]
public class SpellData : ScriptableObject
{
    // Spell Information
    public string spellName = "New Spell";

    [TextArea]
    public string description;

    // Damage Settings
    public DamageData damage;

    // Projectile Settings
    public ProjectileData projectileData;

    // Targeting Settings
    public TargetingData targetingData;

    // Mana Settings
    [Min(0f)]
    public float manaCost = 10f;

    // Casting Settings
    [Min(0f)]
    public float cooldown = 2f;

    // Status Effect
    public StatusEffectData statusEffect;
}
