using UnityEngine;

// MYTHRENOR'S HALLOW - SWORD ATTACK STATE EXIT
//
// PURPOSE:
// Prevents unfinished sword attacks from remaining
// pending after the animation exits.
//
// NOTE:
// Attach this behaviour to the Sword Attack
// state inside the Animator Controller.

public class SwordAttackStateExit : StateMachineBehaviour
{
    // PURPOSE:
    // Clears pending attack data when the
    // Sword Attack state finishes or transitions away.
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CombatInputController combatInput = animator.GetComponentInParent<CombatInputController>();

        if (combatInput != null)
        {
            combatInput.CancelPendingSwordAttack();
        }
    }
}
