using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteraction
{
    [Header("Keys")]
    [SerializeField] private string keyID;
    [SerializeField] private string Name;

    public string PromptInteraction(PlayerInteractions interactions) => $"Grab Item {Name}";
    public bool CanInteract(PlayerInteractions interactions) => !interactions.IsInteracting;

    public void Interact(PlayerInteractions interaction)
    {
        if (interaction.Keys.Contains(keyID) == false)
        {
            interaction.Keys.Add(keyID);
        }

        Destroy(gameObject);
    }
}
