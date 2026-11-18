using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteraction
{
    [Header("Keys")]
    [SerializeField] private string keyID;
    [SerializeField] private string Name;

    [Header("Optional Autosave")]
    [SerializeField] private bool autosaveOnPickup;
    [SerializeField] private string checkpointId = "AfterPuzzle";
    [SerializeField] private Transform spawnPoint;

    public string PromptInteraction(PlayerInteractions interactions)
    {
        return $"Grab Item {Name}";
    }

    public bool CanInteract(PlayerInteractions interactions)
    {
        return !interactions.IsInteracting;
    }

    public void Interact(PlayerInteractions interaction)
    {
        if (!interaction.Keys.Contains(keyID))
        {
            interaction.Keys.Add(keyID);
        }

        if (autosaveOnPickup &&
            SaveManager.Instance != null &&
            spawnPoint != null)
        {
            SaveManager.Instance.Save(checkpointId, spawnPoint);
        }

        Destroy(gameObject);
    }
}
