using UnityEngine;

// MYTHRENOR'S HALLOW - MELEE ATTACK DATA
//
// PURPOSE:
// Stores configurable melee attack settings
// for weapons and Entity abilities.
//
// TODO (JABARI INTEGRATION):
// Allow Jabari's Entities to reference these
// assets through their equipment or abilities.
//
// NOTE:
// This stores attack settings, not combat logic.

[CreateAssetMenu(fileName = "NewMeleeAttack", menuName = "Mythrenors Hallow/Combat/Melee Attack")]
public class MeleeAttackData : ScriptableObject
{
    // Attack Information
    public string attackName = "New Melee Attack";

    [TextArea]
    public string description;

    // Damage Settings
    public DamageData damage;

    // Targeting Settings
    public TargetingData targetingData;

    // Attack Timing
    [Min(0f)]
    public float cooldown = 1f;

    // Attack Requirements
    public bool requiresTarget = true;

    // Status Effect
    public StatusEffectData statusEffect;

    // Melee Visual Effects
    public GameObject swingVfxPrefab;
    public Vector3 swingVfxOffset = new Vector3(0f, 1f, 1f);
    public Vector3 swingVfxRotationOffset = Vector3.zero;

    [Min(0.1f)]
    public float swingVfxLifetime = 2f;

    // Melee Hit Visual Effects
    public GameObject hitVfxPrefab;
    public Vector3 hitVfxRotationOffset = Vector3.zero;

    [Min(0.1f)]
    public float hitVfxLifetime = 2f;
}
