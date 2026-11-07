using UnityEngine;

// MYTHRENOR'S HALLOW - PLAYER COMBAT ANIMATION EVENTS
//
// PURPOSE:
// Receives events from the player's combat animations.
//
// NOTE:
// The script belongs on the same GameObject as the Animator.
//
// TODO:
// Connect SwordHitFrame to the MeleeManager once
// the animation event has been tested.

public class PlayerCombatAnimationEvents : MonoBehaviour
{
    // Combat Input Reference
    [SerializeField] private CombatInputController combatInput;

    // PURPOSE:
    // Find the combat input controller on the character root.
    private void Awake()
    {
        if (combatInput == null)
        {
            combatInput = GetComponentInParent<CombatInputController>();
        }
    }

    // PURPOSE:
    // Called by the sword animation's hit-frame event.
    public void SwordHitFrame()
    {
        if (combatInput != null)
        {
            combatInput.ResolveSwordHitFrame();
        }
    }
}
