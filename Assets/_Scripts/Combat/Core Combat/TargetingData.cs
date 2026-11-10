using UnityEngine;

// MYTHRENOR'S HALLOW - TARGETING DATA
//
// PURPOSE:
// Stores configurable targeting rules for
// melee attacks, ranged weapons, and spells.
//
// TODO (JABARI INTEGRATION):
// Confirm targeting types and faction rules
// with Jabari's Player and Enemy systems.

public enum TargetType
{
    Self,
    SingleEnemy,
    SingleAlly,
    NearestEnemy,
    AllEnemiesInRadius,
    Directional
}

[CreateAssetMenu(fileName = "NewTargetingData", menuName = "Mythrenors Hallow/Combat/Targeting Data")]
public class TargetingData : ScriptableObject
{
    // Targeting Settings
    public TargetType targetType = TargetType.SingleEnemy;

    [Min(0f)]
    public float maxRange = 5f;

    [Min(0f)]
    public float radius = 3f;

    [Range(0f, 360f)]
    public float attackAngle = 120f;

    // Detection Settings
    public LayerMask targetLayers = ~0;

    public bool requireLineOfSight = false;

    public LayerMask obstructionLayers = ~0;

    // Targeting Rules
    public bool allowSelfTarget = false;
}
