using UnityEngine;

// PURPOSE:
// Cancels unfinished melee attacks when the
// Attack animation exits or gets interrupted.

public class EnemyAttackStateExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyMeleeAnimationEvents eventsComponent = animator.GetComponent<EnemyMeleeAnimationEvents>();

        if (eventsComponent != null)
        {
            eventsComponent.CancelMeleeAttack();
        }
    }
}
