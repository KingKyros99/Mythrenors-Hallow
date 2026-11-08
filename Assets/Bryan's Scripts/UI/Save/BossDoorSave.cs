using UnityEngine;

public class BossDoorSave : MonoBehaviour
{
    public string roomId = "bossdoor";
    public Transform gate;            // Level1 (1) > NewLevelDesign > BossDoor
    public Transform spawnPoint;      // BossDoorSpawn (gets moved to the player when saving)
    public float saveDistance = 8f;   // flat distance to the gate, ignores height

    Transform player;
    Renderer[] playerRenderers;
    Collider gateCollider;
    bool done;

    void Start()
    {
        if (gate != null) gateCollider = gate.GetComponentInChildren<Collider>();
    }

    void Update()
    {
        if (done || gate == null) return;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            player = p.transform;
            playerRenderers = p.GetComponentsInChildren<Renderer>();
        }

        Vector3 g = gateCollider != null ? gateCollider.bounds.center : gate.position;
        Vector3 body = PlayerBodyPosition();
        float dist = Vector2.Distance(new Vector2(body.x, body.z), new Vector2(g.x, g.z));

        if (dist <= saveDistance) TrySave();
    }

    Vector3 PlayerBodyPosition()
    {
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers)
                if (r != null && r.enabled && r is SkinnedMeshRenderer) return r.bounds.center;
            foreach (Renderer r in playerRenderers)
                if (r != null && r.enabled) return r.bounds.center;
        }
        return player.position;
    }

    void TrySave()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[Save] Door: no SaveManager. Start from TitleScreen.");
            return;
        }

        // Save the spot where the player is standing right now (outside the gate)
        if (spawnPoint != null)
            spawnPoint.SetPositionAndRotation(player.position, player.rotation);

        Debug.Log("[Save] Door save at the gate");
        done = true;
        SaveManager.Instance.Save(roomId, spawnPoint);
    }
}