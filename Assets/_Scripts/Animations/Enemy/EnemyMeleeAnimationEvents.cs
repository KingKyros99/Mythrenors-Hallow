using UnityEngine;

// MYTHRENOR'S HALLOW - ENEMY MELEE ANIMATION EVENTS
//
// PURPOSE:
// Connects animation impact frames to the existing
// MeleeManager damage system.
//
// NOTE:
// Does not calculate or modify combat damage.

public class EnemyMeleeAnimationEvents : MonoBehaviour
{
    // References
    [SerializeField] private MeleeManager meleeManager;
    [SerializeField] private CombatEntityAdapter combatEntity;

    // PURPOSE:
    // Called by an Animation Event when the weapon hits.
    public void ApplyMeleeHit()
    {
        if (meleeManager == null || combatEntity == null)
        {
            return;
        }

        meleeManager.ResolvePendingAnimatedAttack(combatEntity);
    }

    // PURPOSE:
    // Removes an attack that never reached its hit frame.
    public void CancelMeleeAttack()
    {
        if (meleeManager == null || combatEntity == null)
        {
            return;
        }

        meleeManager.CancelPendingAnimatedAttack(combatEntity);
    }

    private void OnDisable()
    {
        CancelMeleeAttack();
    }
}
