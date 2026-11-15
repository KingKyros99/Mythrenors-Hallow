using UnityEngine;

// PURPOSE:
// Cancels an unfinished ranged attack when
// the shooting animation exits.

public class EnemyRangedStateExit : StateMachineBehaviour
{
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        EnemyRangedAnimationEvents eventsComponent =
            animator.GetComponent<EnemyRangedAnimationEvents>();

        if (eventsComponent != null)
        {
            eventsComponent.CancelCrossbowShot();
        }
    }
}
