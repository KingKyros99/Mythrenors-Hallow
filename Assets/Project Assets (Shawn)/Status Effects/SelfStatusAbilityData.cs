using UnityEngine;

// MYTHRENOR'S HALLOW - SELF STATUS ABILITY DATA
//
// PURPOSE:
// Stores configurable self-targeted abilities
// that apply beneficial status effects.
//
// NOTE:
// Shared assets contain settings, not runtime cooldowns.

[CreateAssetMenu(fileName = "NewSelfStatusAbility", menuName = "Mythrenors Hallow/Combat/Self Status Ability")]
public class SelfStatusAbilityData : ScriptableObject
{
    // Ability Information 
    public string abilityName = "New Ability";

    [TextArea]
    public string description;

    // Status Effect
    public StatusEffectData statusEffect;

    // Resource Settings
    [Min(0f)]
    public float manaCost = 10f;

    // Cooldown
    [Min(0f)]
    public float cooldown = 5f;
}
