using UnityEngine;

// MYTHRENOR'S HALLOW - STATUS EFFECT DATA
//
// PURPOSE:
// Stores configurable status effect settings
// for player and enemy combat abilities.
//
// NOTE:
// Effect definitions are shared ScriptableObjects.
// Runtime duration and timers belong to each Entity.

public enum StatusEffectType
{
    Poison,
    Burn,
    Bleed,
    Regeneration,
    Stun,
    Slow,
    AttackBuff,
    DefenseBuff
}

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Mythrenors Hallow/Combat/Status Effect")]
public class StatusEffectData : ScriptableObject
{
    // Effect Information
    public string effectName = "New Status Effect";

    [TextArea]
    public string description;

    public StatusEffectType effectType;

    //Duration Settings
    [Min(0.1f)]
    public float duration = 5f;

    [Min(0.1f)]
    public float tickInterval = 1f;

    // Effect Strength 
    [Min(0f)]
    public float effectPower = 5f;

    // Stacking
    public bool canStack = false;

    [Min(1)]
    public int maxStacks = 1;
}
