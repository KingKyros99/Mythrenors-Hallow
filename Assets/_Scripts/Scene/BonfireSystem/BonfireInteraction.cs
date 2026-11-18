using UnityEngine;

public class BonfireInteraction : MonoBehaviour, IInteraction
{
    [SerializeField] private string BonfireId;
    [SerializeField] private Transform savePoint;
    [SerializeField] private GameObject bonfireRestScreen;

    private PlayerInteractions player;
    private bool isResting;

    private float previousTimeScale;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    public string PromptInteraction(PlayerInteractions interaction)
    {
        return "Rest And Save?";
    }

    public bool CanInteract(PlayerInteractions interaction)
    {
        return !isResting && !interaction.IsInteracting;
    }

    public void Interact(PlayerInteractions interaction)
    {
        if (!CanInteract(interaction) || bonfireRestScreen == null)
            return;

        previousTimeScale = Time.timeScale;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        player = interaction;
        player.IsInteracting = true;
        isResting = true;

        PlayerRest(interaction);

        if (SaveManager.Instance != null)
        {
            Transform savePosition =
                savePoint != null ? savePoint : transform;

            SaveManager.Instance.Save(BonfireId, savePosition);
        }

        bonfireRestScreen.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitBonfire()
    {
        if (!isResting)
            return;

        FinishRest();

        if (bonfireRestScreen != null)
            bonfireRestScreen.SetActive(false);
    }

    private void OnDisable()
    {
        if (!isResting)
            return;

        FinishRest();

        if (bonfireRestScreen != null)
            bonfireRestScreen.SetActive(false);
    }

    private void FinishRest()
    {
        isResting = false;

        Time.timeScale = previousTimeScale;

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;

        if (player != null)
        {
            player.IsInteracting = false;
            player = null;
        }
    }

    private void PlayerRest(PlayerInteractions interaction)
    {
        CharacterBase character =
            interaction.GetComponent<CharacterBase>();

        if (character == null)
            return;

        character.Heal(character.MaxHealth);
        character.RestoreMana(character.MaxMana);
        character.RestoreStamina(character.MaxStamina);
    }
}