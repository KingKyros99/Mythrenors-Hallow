using UnityEngine;

public class BossDoorSave : MonoBehaviour
{
    public string roomId = "bossdoor";
    public Transform gate;
    public Transform spawnPoint;
    public float saveDistance = 8f;

    private Transform player;
    private Renderer[] playerRenderers;
    private Collider gateCollider;
    private bool done;

    private void Start()
    {
        if (gate != null)
            gateCollider = gate.GetComponentInChildren<Collider>();
    }

    private void Update()
    {
        if (done || gate == null) return;

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null) return;

            player = playerObject.transform;
            playerRenderers =
                playerObject.GetComponentsInChildren<Renderer>();
        }

        Vector3 gatePosition = gateCollider != null
            ? gateCollider.bounds.center
            : gate.position;

        Vector3 bodyPosition = PlayerBodyPosition();

        float distance = Vector2.Distance(
            new Vector2(bodyPosition.x, bodyPosition.z),
            new Vector2(gatePosition.x, gatePosition.z)
        );

        if (distance <= saveDistance)
            TrySave();
    }

    private Vector3 PlayerBodyPosition()
    {
        if (playerRenderers != null)
        {
            foreach (Renderer playerRenderer in playerRenderers)
            {
                if (playerRenderer != null &&
                    playerRenderer.enabled &&
                    playerRenderer is SkinnedMeshRenderer)
                {
                    return playerRenderer.bounds.center;
                }
            }

            foreach (Renderer playerRenderer in playerRenderers)
            {
                if (playerRenderer != null && playerRenderer.enabled)
                    return playerRenderer.bounds.center;
            }
        }

        return player.position;
    }

    private void TrySave()
    {
        if (SaveManager.Instance == null) return;

        if (spawnPoint != null)
        {
            spawnPoint.SetPositionAndRotation(
                player.position,
                player.rotation
            );
        }

        done = true;
        SaveManager.Instance.Save(roomId, spawnPoint);
    }
}