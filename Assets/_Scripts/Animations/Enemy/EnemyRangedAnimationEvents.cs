using UnityEngine;

// MYTHRENOR'S HALLOW - ENEMY RANGED ANIMATION EVENTS
//
// PURPOSE:
// Connects ranged attack animations to the enemy's
// existing projectile firing system.
//
// NOTE:
// Reusable for enemies using animated ranged attacks.
// Does not modify damage, targeting, or projectile logic.

public class EnemyRangedAnimationEvents : MonoBehaviour
{
    // References
    [SerializeField] private Enemy enemy;

    private void Awake()
    {
        // Find the enabled Enemy component if not assigned.
        if (enemy == null || !enemy.isActiveAndEnabled)
        {
            Enemy[] enemies = GetComponentsInParent<Enemy>(true);

            foreach (Enemy candidate in enemies)
            {
                if (candidate.isActiveAndEnabled)
                {
                    enemy = candidate;

                    break;
                }
            }
        }
    }

    // PURPOSE:
    // Called when the animation reaches the firing frame.
    public void FireCrossbowBolt()
    {
        if (enemy == null || !enemy.isActiveAndEnabled)
        {
            return;
        }

        enemy.FireAnimatedRangedShot();
    }

    // PURPOSE:
    // Cancels a shot if the animation is interrupted.
    public void CancelCrossbowShot()
    {
        if (enemy != null)
        {
            enemy.CancelAnimatedRangedShot();
        }
    }

    private void OnDisable()
    {
        CancelCrossbowShot();
    }
}
