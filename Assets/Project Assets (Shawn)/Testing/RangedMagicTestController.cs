
using UnityEngine;

// MYTHRENOR'S HALLOW - RANGED MAGIC TEST CONTROLLER
//
// PURPOSE:
// Tests ranged weapons and projectile spells
// without Jabari's Entity implementation.
//
// NOTE:
// TESTING ONLY. This script should not
// be used in the final gameplay scene.

public class RangedMagicTestController : MonoBehaviour
{
    // Test Entities
    public MockCombatEntity attacker;
    public MockCombatEntity target;

    // Manager References
    public RangedAttackManager rangedManager;
    public SpellManager spellManager;

    // Attack Configurations
    public RangedAttackData rangedAttack;
    public SpellData spell;

    // Test Settings
    public bool testSpells = false;

    [Min(0.1f)]
    public float testInterval = 2f;

    private float nextTestTime;

    private void Update()
    {
        if (Time.time < nextTestTime)
        {
            return;
        }

        nextTestTime = Time.time + testInterval;

        if (attacker == null || target == null)
        {
            return;
        }

        if (!attacker.IsAlive || !target.IsAlive)
        {
            return;
        }

        Vector3 spawnPosition = attacker.transform.position + Vector3.up * 0.5f;

        if (testSpells)
        {
            if (spellManager == null || spell == null)
            {
                return;
            }

            bool successful = spellManager.TryCast(attacker, target, spell, spawnPosition);

            if (successful)
            {
                Debug.Log("Spell test cast successfully.");
            }
        }
        else
        {
            if (rangedManager == null || rangedAttack == null)
            {
                return;
            }

            bool successful = rangedManager.TryFire(attacker, target, rangedAttack, spawnPosition);

            if (successful)
            {
                Debug.Log("Ranged weapon test fired successfully.");
            }
        }
    }
}
