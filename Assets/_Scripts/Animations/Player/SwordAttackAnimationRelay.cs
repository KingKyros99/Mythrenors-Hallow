using UnityEngine;

// MYTHRENOR'S HALLOW - SWORD ATTACK ANIMATION RELAY
//
// PURPOSE:
// Passes the sword animation's hit event
// to the existing combat input controller.

public class SwordAttackAnimationRelay : MonoBehaviour
{
    [SerializeField] private CombatInputController combatInput;

    private void Awake()
    {
        if (combatInput == null)
        {
            combatInput = GetComponentInParent<CombatInputController>();
        }
    }

    // Called by the sword thrust Animation Event.
    public void ResolveSwordHitFrame()
    {
        Debug.Log("Sword thrust hit frame reached!");

        if (combatInput != null)
        {
            combatInput.ResolveSwordHitFrame();
        }

        else
        {
            Debug.LogWarning("CombatInputController not found.");
        }
    }
}
