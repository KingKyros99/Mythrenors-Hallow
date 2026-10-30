using UnityEngine;

// MYTHRENOR'S HALLOW - EXPLICIT TARGET TEST CONTROLLER
//
// PURPOSE:
// Tests whether melee combat attacks a specifically
// selected enemy rather than the nearest enemy.
//
// NOTE:
// TESTING ONLY. Not used in gameplay scenes.

public class ExplicitTargetTestController : MonoBehaviour
{
    // Test References
    [SerializeField] private CombatEntityAdapter attacker;
    [SerializeField] private CombatEntityAdapter selectedTarget;
    [SerializeField] private MeleeManager meleeManager;
    [SerializeField] private MeleeAttackData attackData;

    // PURPOSE:
    // Attempts a melee attack against the selected target.
    [ContextMenu("Test Explicit Target")]
    public void TestExplicitTarget()
    {
        if (attacker == null || selectedTarget == null || meleeManager == null || attackData == null)
        {
            Debug.LogWarning("Explicit target test references are missing.");

            return;
        }

        bool successful = meleeManager.TryAttack(attacker, selectedTarget, attackData);

        if (successful)
        {
            Debug.Log($"PASS: Explicit attack executed against {selectedTarget.name}.");
        }

        else
        {
            Debug.LogWarning("CHECK: Explicit attack was rejected.");
        }
    }
}
