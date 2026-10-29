using UnityEngine;

//  MYTHRENOR'S HALLOW - COMBAT INTERFACES
// 
//  PURPOSE:
//  Defines the information and functions required by
//  Shawn's reusable combat managers.
// 
//  TODO (JABARI INTEGRATION):
//  Confirm compatibility with:
//  - CharacterStats.cs
//  - CoreFunctions.cs
//  - Player.cs
//  - Enemy.cs
// 
//  Jabari may implement this interface directly
//  or through a separate adapter component.
public interface ICombatEntity
{
    // PURPOSE:
    // Provides the Entity's Transform for targeting,
    // attack range calculations, and combat feedback.
    //
    // TODO (JABARI INTEGRATION):
    // Connect to Jabari's Player/Enemy Transform.
    Transform CombatTransform { get; }

    // PURPOSE:
    // Identifies whether the Entity can participate
    // in combat and which faction it belongs to.
    //
    // TODO (JABARI INTEGRATION):
    // Connect these to Jabari's character state
    // and team/faction system.
    bool IsAlive { get; }
    int TeamId { get; }

    // PURPOSE:
    // Supplies combat statistics without storing
    // or modifying them inside Shawn's managers.
    //
    // TODO (JABARI INTEGRATION):
    // Map these values to CharacterStats.cs.
    float Attack { get; }
    float Defense { get; }

    float CriticalChance { get; }
    float CriticalMultiplier { get; }

    // PURPOSE:
    // Requests damage or healing from an Entity.
    // The Entity itself controls its health.
    //
    // TODO (JABARI INTEGRATION):
    // Connect to Jabari's health functions.
    void ReceiveDamage(float amount);
    void ReceiveHealing(float amount);

    // PURPOSE:
    // Requests resource consumption for spellcasting.
    // Returns false when mana is insufficient.
    //
    // TODO (JABARI INTEGRATION):
    // Confirm mana storage and spending logic.
    bool TrySpendMana(float amount);
}
