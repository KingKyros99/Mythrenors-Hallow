
using UnityEngine;

// MYTHRENOR'S HALLOW - HEALING TEST CONTROLLER
//
// PURPOSE:
// Tests damage and healing through the shared
// combat framework using Jabari's CharacterBase.
//
// NOTE:
// TESTING ONLY. This is not a gameplay script.

public class HealingTestController : MonoBehaviour
{
    [Header("Test References")]
    [SerializeField] private CombatEntityAdapter target;
    [SerializeField] private CharacterBase characterBase;

    [Header("Test Values")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float healingAmount = 50f;

    // PURPOSE:
    // Tests whether healing stays within MaxHealth.
    [ContextMenu("Test Healing Maximum")]
    public void TestHealingMaximum()
    {
        if (target == null || characterBase == null)
        {
            Debug.LogWarning("Healing test references are missing.");
            return;
        }

        if (!target.IsAlive)
        {
            Debug.LogWarning("Healing test requires a living Entity.");
            return;
        }

        // Damage the character first.
        target.ReceiveDamage(damageAmount);

        Debug.Log($"Health Before Healing: {characterBase.CurrentHealth}/{characterBase.MaxHealth}");

        // Request healing through CombatManager.
        CombatManager.TryHeal(target, healingAmount);

        Debug.Log($"Health After Healing: {characterBase.CurrentHealth}/{characterBase.MaxHealth}");

        // Verify that healing did not exceed maximum.
        if (characterBase.CurrentHealth == characterBase.MaxHealth)
        {
            Debug.Log("PASS: Healing stopped at maximum health.");
        }
        else
        {
            Debug.LogWarning("CHECK: Healing did not reach maximum health.");
        }
    }
}
