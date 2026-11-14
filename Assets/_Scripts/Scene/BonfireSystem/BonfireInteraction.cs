using UnityEngine;

public class BonfireInteraction : MonoBehaviour, IInteraction
{
    [SerializeField] private string BonfireId;
    [SerializeField] private Transform savePoint;



    [SerializeField] private GameObject bonfireRestScreen;

    PlayerInteractions player;
    public string PromptInteraction(PlayerInteractions interaction) => "Rest And Save?";
    public bool CanInteract(PlayerInteractions interaction) => !interaction.IsInteracting;

    public void Interact(PlayerInteractions interaction)
    {
        player = interaction;
        player.IsInteracting = true;
        //Player Rest
        PlayerRest(interaction);
        //Save System
        if (SaveManager.Instance != null)
        {
            Transform savePosition = savePoint != null ? savePoint : transform;
            SaveManager.Instance.Save(BonfireId, savePosition);
        }

        bonfireRestScreen.SetActive(true);
        //Unlock Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitBonfire()
    {
        bonfireRestScreen.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (player != null)
        {
            player.IsInteracting = false;
            player = null;
        }
    }


    private void PlayerRest(PlayerInteractions interaction)
    {
        CharacterBase player = interaction.GetComponent<CharacterBase>();
        if (player != null)
        {
            player.Heal(player.MaxHealth);
            player.RestoreMana(player.MaxMana);
            player.RestoreStamina(player.MaxStamina);
        }
    }

}
