
using UnityEngine;

// MYTHRENOR'S HALLOW - MELEE TEST CONTROLLER
//
// PURPOSE:
// Runs a temporary melee combat test without
// relying on Jabari's player input or AI.
//
// NOTE:
// TESTING ONLY. Remove from gameplay scenes
// when the actual Entity integration is ready.

public class MeleeTestController : MonoBehaviour
{
    [Header("Test References")]
    public MockCombatEntity attacker;
    public MockCombatEntity target;

    public MeleeManager meleeManager;
    public MeleeAttackData attackData;

    // PURPOSE:
    // Tries one attack each time the configured
    // automatic test interval passes.
    //
    // Uses automatic testing so it does not
    // depend on Unity's input configuration.
    [Min(0.1f)]
    public float testInterval = 1f;

    private float nextTestTime;

    private void Update()
    {
        if (Time.time < nextTestTime)
        {
            return;
        }

        nextTestTime = Time.time + testInterval;

        if (attacker == null || target == null ||
            meleeManager == null || attackData == null)
        {
            return;
        }

        if (!target.IsAlive)
        {
            return;
        }

        bool successful = meleeManager.TryAttack(
            attacker,
            target,
            attackData
        );

        if (successful)
        {
            Debug.Log("Melee test attack succeeded.");
        }
    }
}
