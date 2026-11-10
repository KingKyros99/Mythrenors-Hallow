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
    public UnityEvent onPaused;
    public UnityEvent onResumed;
    public UnityEvent onBeforeQuitToTitle;

    public static bool OpenSettingsOnLoad;

    InputAction pauseAction;
    bool isPaused;
    int lastSettingsCloseFrame = -1;

    void Awake()
    {
        pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        pauseAction.performed += _ => OnPausePressed();
    }

    void OnEnable() { pauseAction.Enable(); }
    void OnDisable() { pauseAction.Disable(); }

    void OnDestroy()
    {
        pauseAction.Disable();
        pauseAction.Dispose();
    }

    void Start()
    {
        pausePanel.SetActive(false);
        isPaused = false;
    }

    void OnPausePressed()
    {
        if (Time.frameCount == lastSettingsCloseFrame) return;
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        onPaused.Invoke();
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        onResumed.Invoke();
    }

    public void OpenSettings()
    {
        if (settingsMenu == null) return;
        pausePanel.SetActive(false);
        settingsMenu.Open(() =>
        {
            lastSettingsCloseFrame = Time.frameCount;
            pausePanel.SetActive(true);
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

    void LeaveToTitle()
    {
        onBeforeQuitToTitle.Invoke();
        isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        onResumed.Invoke();
        SceneManager.LoadScene(titleSceneName);
    }
}