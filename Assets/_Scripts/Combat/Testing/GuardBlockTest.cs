using System.Collections;
using UnityEngine;

// MYTHRENOR'S HALLOW - GUARD BLOCK TEST
//
// PURPOSE:
// Temporarily tests successful blocking and
// Guard Impact VFX without enemy attacks.
//
// NOTE:
// Testing only. Remove after verification.

public class GuardBlockTest : MonoBehaviour
{
    // References
    [SerializeField] private CoreFunctions coreFunctions;

    // Test Settings
    [SerializeField] private float testDamage = 10f;

    // PURPOSE:
    // Simulates an incoming blockable attack.
    [ContextMenu("Test Successful Guard Block")]
    private void TestSuccessfulGuardBlock()
    {
        if (!Application.isPlaying || coreFunctions == null)
        {
            Debug.LogWarning("Enter Play Mode and assign CoreFunctions.");

            return;
        }

        StartCoroutine(RunBlockTest());
    }

    private IEnumerator RunBlockTest()
    {
        bool wasBlocking = coreFunctions.IsBlocking;

        // Temporarily enable guarding.
        if (!wasBlocking)
        {
            coreFunctions.SetBlocking(true);
        }

        yield return new WaitForSeconds(0.2f);

        // Attempt a successful block.
        bool blocked = coreFunctions.Block(testDamage, false, out float remainingDamage);

        Debug.Log($"Block Successful: {blocked}, " + $"Remaining Damage: {remainingDamage}");

        yield return new WaitForSeconds(0.5f);

        // Restore the original guard state.
        if (!wasBlocking)
        {
            coreFunctions.SetBlocking(false);
        }
    }
}
