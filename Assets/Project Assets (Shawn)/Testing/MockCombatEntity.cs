
using UnityEngine;

// MYTHRENOR'S HALLOW - MOCK COMBAT ENTITY
//
// PURPOSE:
// Provides temporary character stats for testing
// Shawn's combat managers without Jabari's Entity.
//
// NOTE:
// TESTING ONLY. This is not a replacement
// for Jabari's CharacterStats or Entity system.
//
// TODO (JABARI INTEGRATION):
// Replace this test implementation with
// Jabari's actual Entity or combat adapter.

public class MockCombatEntity : MonoBehaviour, ICombatEntity
{
    [Header("Test Character Settings")]
    public int teamId = 0;

    public float maxHealth = 100f;
    public float currentHealth = 100f;

    public float attack = 15f;
    public float defense = 5f;

    public float criticalChance = 0f;
    public float criticalMultiplier = 2f;

    public float mana = 50f;

    // Provide test data to the combat managers.
    public Transform CombatTransform => transform;

    public bool IsAlive => currentHealth > 0f;

    public int TeamId => teamId;

    public float Attack => attack;
    public float Defense => defense;

    public float CriticalChance => criticalChance;
    public float CriticalMultiplier => criticalMultiplier;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // PURPOSE:
    // Simulates Jabari's damage handling.
    public void ReceiveDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);

        Debug.Log($"{name} HP: {currentHealth}/{maxHealth}");
    }

    // PURPOSE:
    // Simulates Jabari's healing handling.
    public void ReceiveHealing(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    // PURPOSE:
    // Simulates spending mana for future spell tests.
    public bool TrySpendMana(float amount)
    {
        if (amount < 0f || mana < amount)
        {
            return false;
        }

        mana -= amount;

        return true;
    }
}
