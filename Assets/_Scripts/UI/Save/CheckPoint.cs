using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointId = "Checkpoint_01";
    [SerializeField] private Transform spawnPoint;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterBase player =
            other.GetComponentInParent<CharacterBase>();

        if (player == null)
            return;

        bool isPlayer =
            other.CompareTag("Player") ||
            player.CompareTag("Player") ||
            player.transform.root.CompareTag("Player");

        if (!isPlayer)
            return;

        if (SaveManager.Instance == null || spawnPoint == null)
            return;

        SaveManager.Instance.Save(checkpointId, spawnPoint);
    }
}