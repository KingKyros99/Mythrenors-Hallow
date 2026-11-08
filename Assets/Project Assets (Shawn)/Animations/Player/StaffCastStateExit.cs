using UnityEngine;

// MYTHRENOR'S HALLOW - STAFF CAST STATE EXIT
//
// PURPOSE:
// Clears an unfinished staff attack when
// the StaffCast animation ends or is interrupted.

public class StaffCastStateExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CombatInputController combatInput = animator.GetComponentInParent<CombatInputController>();

        if (combatInput != null)
        {
            combatInput.CancelPendingStaffCast();
        }
    }
}
