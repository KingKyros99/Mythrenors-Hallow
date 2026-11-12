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

    [Header("Events (teammates hook these)")]
    public UnityEvent onPaused = new UnityEvent();
    public UnityEvent onResumed = new UnityEvent();
    public UnityEvent onBeforeQuitToTitle = new UnityEvent();

    public static bool OpenSettingsOnLoad;

    public bool IsPaused => isPaused;

    private InputAction pauseAction;
    private bool isPaused;
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

        if (isPaused)
            Resume();
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

        isPaused = false;
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        OnPausePressed();
    }

    private void OnPausePressed()
    {
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
        if (isPaused) return;

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
        if (!isPaused) return;

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
        if (settingsMenu == null) return;
        if (settingsMenu.IsOpen) return;

        if (!isPaused)
            Pause();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        settingsMenu.Open(() =>
        {
            lastSettingsCloseFrame = Time.frameCount;

            if (isPaused)
            {
                Time.timeScale = 0f;

                if (pausePanel != null)
                    pausePanel.SetActive(true);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        });
    }

    public void OpenSettingsViaTitle()
    {
        OpenSettingsOnLoad = true;
        LeaveToTitle();
    }

    public void QuitToTitle()
    {
        LeaveToTitle();
    }

    private void LeaveToTitle()
    {
        onBeforeQuitToTitle.Invoke();

        isPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        onResumed.Invoke();

        SceneManager.LoadScene(titleSceneName);
    }
}