using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public SettingsMenuUI settingsMenu;

    [Header("Scenes")]
    public string titleSceneName = "TitleScreen";

    [Header("Loading screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField, Min(0f)] private float minimumLoadingTime = 3f;

    [Header("Events (teammates hook these)")]
    public UnityEvent onPaused = new UnityEvent();
    public UnityEvent onResumed = new UnityEvent();
    public UnityEvent onBeforeQuitToTitle = new UnityEvent();

    public static bool OpenSettingsOnLoad;

    public bool IsPaused => isPaused;

    private InputAction pauseAction;
    private bool isPaused;
    private bool leavingToTitle;
    private float previousTimeScale = 1f;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private int lastSettingsCloseFrame = -1;

    private void Awake()
    {
        pauseAction = new InputAction(
            "Pause",
            InputActionType.Button,
            "<Keyboard>/escape"
        );

        pauseAction.performed += OnPauseInput;
    }

    private void OnEnable()
    {
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.Disable();

        if (isPaused && !leavingToTitle)
        {
            isPaused = false;
            Time.timeScale = previousTimeScale;

            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;

            if (pausePanel != null)
                pausePanel.SetActive(false);

            onResumed.Invoke();
        }
    }

    private void OnDestroy()
    {
        pauseAction.performed -= OnPauseInput;
        pauseAction.Dispose();
    }

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        isPaused = false;
        leavingToTitle = false;
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        OnPausePressed();
    }

    private void OnPausePressed()
    {
        if (leavingToTitle) return;

        if (Time.frameCount <= lastSettingsCloseFrame + 1)
            return;

        if (settingsMenu != null && settingsMenu.IsOpen)
            return;

        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (isPaused || leavingToTitle) return;

        previousTimeScale = Time.timeScale;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        onPaused.Invoke();
    }

    public void Resume()
    {
        if (!isPaused || leavingToTitle) return;

        if (settingsMenu != null && settingsMenu.IsOpen)
            return;

        isPaused = false;
        Time.timeScale = previousTimeScale;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;

        onResumed.Invoke();
    }

    public void OpenSettings()
    {
        if (leavingToTitle || settingsMenu == null) return;
        if (settingsMenu.IsOpen) return;

        if (!isPaused)
            Pause();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        settingsMenu.Open(() =>
        {
            lastSettingsCloseFrame = Time.frameCount;

            if (leavingToTitle || !isPaused)
                return;

            Time.timeScale = 0f;

            if (pausePanel != null)
                pausePanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        });
    }

    public void OpenSettingsViaTitle()
    {
        if (leavingToTitle) return;

        if (!CanLoadTitle()) return;

        OpenSettingsOnLoad = true;
        LeaveToTitle();
    }

    public void QuitToTitle()
    {
        if (leavingToTitle) return;

        OpenSettingsOnLoad = false;
        LeaveToTitle();
    }

    private bool CanLoadTitle()
    {
        if (!string.IsNullOrWhiteSpace(titleSceneName) &&
            Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            return true;
        }

        Debug.LogError(
            "PauseMenuUI: check Title Scene Name and include " +
            "that scene in the active Build Profile's scene list.",
            this
        );

        return false;
    }

    private void LeaveToTitle()
    {
        if (leavingToTitle) return;
        if (!CanLoadTitle()) return;

        leavingToTitle = true;
        StartCoroutine(LoadTitle());
    }

    private IEnumerator LoadTitle()
    {
        if (!isPaused)
        {
            previousTimeScale = Time.timeScale;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            isPaused = true;
            onPaused.Invoke();
        }

        Time.timeScale = 0f;
        onBeforeQuitToTitle.Invoke();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (loadingPanel != null)
        {
            loadingPanel.transform.SetAsLastSibling();
            loadingPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;
        yield return null;

        float startedAt = Time.realtimeSinceStartup;

        AsyncOperation loading = null;
        string loadingError = null;

        try
        {
            loading = SceneManager.LoadSceneAsync(titleSceneName);
        }
        catch (System.Exception exception)
        {
            loadingError = exception.Message;
        }

        if (loading == null)
        {
            Debug.LogError(
                "PauseMenuUI: could not load the title scene. " +
                loadingError,
                this
            );

            leavingToTitle = false;
            OpenSettingsOnLoad = false;

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            if (pausePanel != null &&
                (settingsMenu == null || !settingsMenu.IsOpen))
            {
                pausePanel.SetActive(true);
            }

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

        isPaused = false;
        Time.timeScale = 1f;

        onResumed.Invoke();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        loading.allowSceneActivation = true;

        while (!loading.isDone)
            yield return null;
    }
}