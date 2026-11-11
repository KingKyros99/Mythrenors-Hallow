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

    [Header("Loading screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField, Min(0f)] private float minimumLoadingTime = 1f;

    [Header("Teammate hooks")]
    public UnityEvent onContinue = new UnityEvent();
    public UnityEvent onNewGame = new UnityEvent();

    private bool continueAvailable;
    private bool busy;

    private void Awake()
    {
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

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
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

        
        if (onContinue.GetPersistentEventCount() > 0)
        {
            onContinue.Invoke();
            return;
        }

        busy = true;
        onContinue.Invoke();
        StartCoroutine(FadeAndLoad());
    }

    private void OnNewGame()
    {
        if (busy) return;

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
        if (!CanLoadGameplayScene()) return;

        busy = true;

        PlayerPrefs.SetInt(KeyHasGame, 1);
        PlayerPrefs.Save();

        onNewGame.Invoke();
        StartCoroutine(FadeAndLoad());
    }

    private void OnSettings()
    {
        if (busy || settingsMenu == null) return;

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
            EventSystem.current.SetSelectedGameObject(first.gameObject);
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

    private bool CanLoadGameplayScene()
    {
        if (!string.IsNullOrWhiteSpace(gameplaySceneName) &&
            Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            return true;
        }

        Debug.LogError(
            "TitleScreenUI: assign Gameplay Scene Name and include " +
            "that scene in the active Build Profile's scene list.",
            this
        );

        return false;
    }

    
    public void LoadGameplayWithLoadingScreen()
    {
        if (busy) return;

        busy = true;
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        if (!CanLoadGameplayScene())
        {
            busy = false;
            SelectFirstButton();
            yield break;
        }

        busy = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (loadingPanel != null)
        {
            // will take away the fade for scenes with a loading panel, but will still fade for scenes without one.
            if (fadeToBlack != null)
            {
                fadeToBlack.alpha = 0f;
                fadeToBlack.blocksRaycasts = false;
            }

            if (loadingText != null)
                loadingText.text = "Loading...";

            loadingPanel.transform.SetAsLastSibling();
            loadingPanel.SetActive(true);

            // Allows the loading screen to render before loading starts.
            yield return null;
            yield return null;
        }
        else if (fadeToBlack != null)
        {
            // this will Keep the original fade for scenes without a loading panel.
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

        float startedAt = Time.realtimeSinceStartup;

        AsyncOperation loading = null;
        string loadingError = null;

        try
        {
            loading = SceneManager.LoadSceneAsync(gameplaySceneName);
        }
        catch (System.Exception exception)
        {
            loadingError = exception.Message;
        }

        if (loading == null)
        {
            Debug.LogError(
                "TitleScreenUI: scene loading failed. " + loadingError,
                this
            );

            RestoreMenuAfterLoadingFailure();
            yield break;
        }

        loading.allowSceneActivation = false;

        float minimumTime = loadingPanel != null
            ? Mathf.Max(0f, minimumLoadingTime)
            : 0f;

        while (loading.progress < 0.9f ||
               Time.realtimeSinceStartup - startedAt < minimumTime)
        {
            yield return null;
        }

        Time.timeScale = 1f;
        loading.allowSceneActivation = true;

        while (!loading.isDone)
            yield return null;
    }

    private void RestoreMenuAfterLoadingFailure()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (fadeToBlack != null)
        {
            fadeToBlack.alpha = 0f;
            fadeToBlack.blocksRaycasts = false;
        }

        busy = false;
        SelectFirstButton();
    }

    [ContextMenu("Reset Continue (testing)")]
    private void ResetContinueForTesting()
    {
        PlayerPrefs.DeleteKey(KeyHasGame);
        PlayerPrefs.Save();
        SetContinueAvailable(false);
    }
}