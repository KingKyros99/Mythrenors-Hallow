
using System.Collections.Generic;
using UnityEngine;

// MYTHRENOR'S HALLOW - AOE TEST CONTROLLER
//
// PURPOSE:
// Tests area-of-effect targeting with multiple enemies.
//
// NOTE:
// TESTING ONLY. Does not apply damage.

public class AOETestController : MonoBehaviour
{
    [SerializeField] private CombatEntityAdapter caster;
    [SerializeField] private CombatEntityAdapter centerTarget;
    [SerializeField] private TargetingData targetingData;

    [ContextMenu("Test AOE Targeting")]
    public void TestAOETargeting()
    {
        if (caster == null || centerTarget == null || targetingData == null)
        {
            Debug.LogWarning("AOE test references are missing.");
            return;
        }

        Vector3 center = centerTarget.CombatTransform.position;

        List<ICombatEntity> targets =
            TargetingSystem.FindEnemiesInRadius(caster, center, targetingData);

        Debug.Log($"AOE detected {targets.Count} enemies.");

        foreach (ICombatEntity target in targets)
        {
            Debug.Log($"AOE Target: {target.CombatTransform.name}");
        }
    }
}
