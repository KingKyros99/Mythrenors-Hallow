using UnityEngine;

public class WallPuzzle : MonoBehaviour
{
    [Header("Puzzle Detection")]
    [SerializeField] private string fireBall = "FireBall";


    private bool isBroken = false;

    private bool IsFireball(GameObject obj)
    {
        return obj.CompareTag(fireBall) || obj.transform.parent != null && obj.transform.parent.CompareTag(fireBall);
    }

    public void ProjectileHit(GameObject obj)
    {
        if (obj != null || IsFireball(obj.gameObject))
        {
            fireballHit();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (IsFireball(collision.gameObject))
        {
            fireballHit();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsFireball(other.gameObject))
        {
            fireballHit();
        }
    }

    private void fireballHit()
    {
        if (isBroken)
        {
            return;
        }
        BreakWall();
    }
    private void BreakWall()
    {
        if (isBroken) return;
        isBroken = true;
        Destroy(gameObject);
    }


}
