using UnityEngine;

public class SaveButtons : MonoBehaviour
{
    [SerializeField] private TitleScreenUI titleScreenUI;

    private void Awake()
    {
        if (titleScreenUI == null)
            titleScreenUI = GetComponent<TitleScreenUI>();
    }

    public void Continue()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "SaveButtons: no SaveManager found.",
                this
            );
            return;
        }

        if (titleScreenUI == null)
        {
            Debug.LogError(
                "SaveButtons: assign Title Screen UI in the Inspector.",
                this
            );
            return;
        }

        if (!SaveManager.Instance.PrepareContinue())
            return;

        titleScreenUI.LoadGameplayWithLoadingScreen();
    }

    public void NewGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.DeleteSave();
    }
}