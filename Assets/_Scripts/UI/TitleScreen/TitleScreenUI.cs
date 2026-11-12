using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    public const string KeyHasGame = "mh.save.hasGame";

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Continue greyed-out look")]
    [SerializeField] private TMP_Text continueLabel;
    [SerializeField]
    private Color normalTextColor =
        new Color32(0xE8, 0xE1, 0xD0, 0xFF);
    [SerializeField]
    private Color disabledTextColor =
        new Color32(0xE8, 0xE1, 0xD0, 0x55);

    [Header("Panels")]
    [SerializeField] private SettingsMenuUI settingsMenu;
    [SerializeField] private ConfirmDialog confirmDialog;
    [SerializeField] private GameObject[] hideWhileSettingsOpen;

    [Header("Scene loading")]
    [SerializeField] private string gameplaySceneName = "";
    [SerializeField] private CanvasGroup fadeToBlack;
    [SerializeField] private float fadeTime = 1f;

    [Header("Teammate hooks")]
    public UnityEvent onContinue = new UnityEvent();
    public UnityEvent onNewGame = new UnityEvent();

    private bool continueAvailable;
    private bool busy;

    private string clickSfx = "UIClick";

    private void Awake()
    {
        // Buttons are optional so this also works on Win/Lose screens.
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);

        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);

        if (fadeToBlack != null)
        {
            fadeToBlack.alpha = 0f;
            fadeToBlack.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        SetContinueAvailable(PlayerPrefs.GetInt(KeyHasGame, 0) == 1);
        SelectFirstButton();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinue);

        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(OnNewGame);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettings);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuit);
    }

    public void SetContinueAvailable(bool available)
    {
        continueAvailable = available && continueButton != null;

        if (continueButton != null)
            continueButton.interactable = continueAvailable;

        if (continueLabel != null)
        {
            continueLabel.color = continueAvailable
                ? normalTextColor
                : disabledTextColor;
        }
    }

    private void OnContinue()
    {
        if (busy || !continueAvailable) return;

        PlayClick();

        onContinue.Invoke();

        if (onContinue.GetPersistentEventCount() == 0)
            StartCoroutine(FadeAndLoad());
    }

    private void OnNewGame()
    {
        if (busy) return;

        PlayClick();

        if (continueAvailable && confirmDialog != null)
        {
            confirmDialog.Show(
                "Start a new game? Your current progress will be lost.",
                StartNewGame
            );
            return;
        }

        StartNewGame();
    }

    private void StartNewGame()
    {
        if (busy) return;

        PlayClick();

        PlayerPrefs.SetInt(KeyHasGame, 1);
        PlayerPrefs.Save();

        onNewGame.Invoke();
        StartCoroutine(FadeAndLoad());
    }

    private void OnSettings()
    {
        if (busy || settingsMenu == null) return;

        PlayClick();

        SetMainMenuVisible(false);

        settingsMenu.Open(() =>
        {
            SetMainMenuVisible(true);

            if (EventSystem.current != null && settingsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(
                    settingsButton.gameObject
                );
            }
        });
    }

    private void OnQuit()
    {
        if (busy) return;
        PlayClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SelectFirstButton()
    {
        if (EventSystem.current == null) return;

        Button first = continueAvailable ? continueButton : newGameButton;

        if (first == null || !first.interactable)
            first = quitButton;

        if (first != null && first.interactable)
        {
            EventSystem.current.SetSelectedGameObject(first.gameObject);
        }
    }

    private void SetMainMenuVisible(bool visible)
    {
        if (hideWhileSettingsOpen == null) return;

        foreach (GameObject go in hideWhileSettingsOpen)
        {
            if (go != null)
                go.SetActive(visible);
        }
    }

    private IEnumerator FadeAndLoad()
    {
        busy = true;

        if (fadeToBlack != null)
        {
            fadeToBlack.blocksRaycasts = true;

            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeToBlack.alpha = Mathf.Clamp01(elapsed / fadeTime);
                yield return null;
            }

            fadeToBlack.alpha = 1f;
        }

        if (!string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogWarning(
                "TitleScreenUI: assign Gameplay Scene Name in the Inspector.",
                this
            );

            if (fadeToBlack != null)
            {
                fadeToBlack.alpha = 0f;
                fadeToBlack.blocksRaycasts = false;
            }

            busy = false;
        }
    }

    [ContextMenu("Reset Continue (testing)")]
    private void ResetContinueForTesting()
    {
        PlayerPrefs.DeleteKey(KeyHasGame);
        PlayerPrefs.Save();
        SetContinueAvailable(false);
    }

    private void PlayClick()
    {
        AudioManger.inst.PlaySFX(clickSfx);
    }

}