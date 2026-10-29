using UnityEngine;

//  MYTHRENOR'S HALLOW - COMBAT MANAGER
// 
//  PURPOSE:
//  Provides reusable damage and healing operations
//  for melee, ranged weapons, and spells.
//
//  CURRENT LIMITATIONS:
//  - No dodge or blocking calculations yet.
//  - No status effect modifiers yet.
//  - No advanced resistances.
//  - No full targeting integration.
// 
//  TODO (JABARI INTEGRATION):
//  Connect to Jabari's actual Unity Entity
//  stats and damage-reception functions.
public static class CombatManager
{
    //  PURPOSE:
    //  Calculates damage from one Entity to another.
    //
    //  TODO (DESIGN):
    //  Confirm final rules for physical, magical,
    //  and true damage with the team.
    public static float CalculateDamage(ICombatEntity attacker, ICombatEntity defender, DamageData damageData)
    {
        // Prevent calculations with missing entities.
        if (attacker == null || defender == null)
        {
            return 0f;
        }

        // Dead entities cannot attack or receive hits.
        if (!attacker.IsAlive || !defender.IsAlive)
        {
            return 0f;
        }

        // Treat BaseDamage as the configured attack
        // or spell damage bonus.
        float damage = Mathf.Max(0f, attacker.Attack + damageData.BaseDamage);

        // TODO (DESIGN):
        // These defense rules are provisional.
        if (!damageData.IgnoreDefense && damageData.DamageType != DamageType.True)
        {
            damage -= defender.Defense;
        }

        // PROTOTYPE RULE:
        // Successful attacks currently deal at least 1 damage.
        // TODO (DESIGN): Revisit minimum damage when
        // resistance and damage immunity are implemented.
        damage = Mathf.Max(1f, damage);

        // Critical hit calculation.
        if (damageData.CanCrit && Random.value < Mathf.Clamp01(attacker.CriticalChance / 100f))
        {
            damage *= Mathf.Max(1f, attacker.CriticalMultiplier);
        }

        // Convert the result into a rounded value.
        return Mathf.Round(damage);
    }

    //  PURPOSE:
    //  Validates an attack, calculates damage,
    //  and requests that the defender take damage.
    //
    //  TODO (JABARI INTEGRATION):
    //  ReceiveDamage must connect to Jabari's
    //  actual Entity health/damage functions.
    public static bool TryDealDamage(ICombatEntity attacker, ICombatEntity defender, DamageData damageData)
    {
        // Reject missing or defeated entities.
        if (attacker == null || defender == null)
        {
            return false;
        }

        if (!attacker.IsAlive || !defender.IsAlive)
        {
            return false;
        }

        // Prevent friendly fire between entities on the same team.
        // TODO (DESIGN): Confirm team IDs and friendly-fire rules
        // with Jabari when his Entity system is implemented.
        if (attacker.TeamId == defender.TeamId)
        {
            return false;
        }

        // Use the shared damage calculator.
        float damage = CalculateDamage(attacker, defender, damageData);

        if (damage <= 0f)
        {
            return false;
        }

        defender.ReceiveDamage(damage);

        // Temporary Console output for testing.
        // Later, UI or VFX can display damage.
        Debug.Log($"{attacker.CombatTransform.name} dealt " + $"{damage} {damageData.DamageType} damage to " + $"{defender.CombatTransform.name}");

        return true;
    }

    //  PURPOSE:
    //  Requests healing for a valid Entity.
    //
    //  TODO (JABARI INTEGRATION):
    //  Connect healing to CharacterStats.cs
    //  or CoreFunctions.cs.
    public static bool TryHeal(ICombatEntity target, float amount)
    {
        if (target == null || !target.IsAlive || amount <= 0f)
        {
            return false;
        }

        // Healing is handled by Jabari's Entity implementation.
        // TODO (JABARI INTEGRATION): Verify the healing interface.
        target.ReceiveHealing(amount);

        return true;
    }
}
