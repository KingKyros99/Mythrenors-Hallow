using System;
using UnityEngine;

//  MYTHRENOR'S HALLOW - DAMAGE DATA
// 
//  PURPOSE:
//  Stores the information required to calculate
//  damage from weapons, projectiles, and spells.
// 
//  TODO (JABARI INTEGRATION):
//  Confirm which damage types and combat
//  modifiers the team wants to support.
//
// PURPOSE:
// Defines categories of damage.
//
// TODO (DESIGN):
// Physical, Magical, and True are proposed types.
// Confirm final damage categories with the team.
public enum DamageType
{
    Physical,
    Magical,
    True
}

// PURPOSE:
// Allows Unity to serialize damage information.
// Used as an input to CombatManager.
//
// NOTE:
// This is a data container, not a damage calculator.
[Serializable]
public struct DamageData
{
    // Base damage supplied by the attack or spell.
    public float BaseDamage;

    // The category of damage being dealt.
    public DamageType DamageType;

    // Determines whether this attack may crit.
    public bool CanCrit;

    // Determines whether defense is ignored.
    public bool IgnoreDefense;

    // PURPOSE:
    // Creates damage information for an attack.
    //
    // NOTE:
    // These values will eventually come from
    // ScriptableObject weapon/spell configurations.
    public DamageData(float baseDamage, DamageType damageType, bool canCrit = true, bool ignoreDefense = false)
    {
        BaseDamage = baseDamage;

        DamageType = damageType;

        CanCrit = canCrit;

        IgnoreDefense = ignoreDefense;
    }
}
