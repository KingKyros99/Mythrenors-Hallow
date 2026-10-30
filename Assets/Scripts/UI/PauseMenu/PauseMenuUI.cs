using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private SettingsMenuUI settingsMenu;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToTitleButton;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "";

    [Header("Teammate hooks")]
    public UnityEvent onPaused = new UnityEvent();
    public UnityEvent onResumed = new UnityEvent();

    private InputAction pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");

    public bool IsPaused => pausePanel != null && pausePanel.activeSelf;

    private void Awake()
    {
        pausePanel.SetActive(false);
        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);
        quitToTitleButton.onClick.AddListener(QuitToTitle);
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPauseKey;
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPauseKey;
        pauseAction.Disable();
    }

    private int lastSettingsCloseFrame = -1;

    private void OnPauseKey(InputAction.CallbackContext ctx)
    {
        if (settingsMenu != null && settingsMenu.IsOpen) return;
        if (Time.frameCount == lastSettingsCloseFrame) return;
        if (IsPaused) Resume(); else Pause();
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        onPaused.Invoke();
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        onResumed.Invoke();
    }

    private void OpenSettings()
    {
        if (settingsMenu == null) return;
        pausePanel.SetActive(false);
        settingsMenu.Open(() =>
        {
            lastSettingsCloseFrame = Time.frameCount;
            pausePanel.SetActive(true);
        });
    }

    private void QuitToTitle()
    {
        if (string.IsNullOrEmpty(titleSceneName)) return;
        onResumed.Invoke();
        SceneManager.LoadScene(titleSceneName);
    }
}