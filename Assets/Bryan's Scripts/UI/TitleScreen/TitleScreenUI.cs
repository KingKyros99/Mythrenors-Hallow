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
    [SerializeField] private Color normalTextColor = new Color32(0xE8, 0xE1, 0xD0, 0xFF);
    [SerializeField] private Color disabledTextColor = new Color32(0xE8, 0xE1, 0xD0, 0x55);

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

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinue);
        newGameButton.onClick.AddListener(OnNewGame);
        settingsButton.onClick.AddListener(OnSettings);
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

    

    public void SetContinueAvailable(bool available)
    {
        continueAvailable = available;
        continueButton.interactable = available;
        if (continueLabel != null) continueLabel.color = available ? normalTextColor : disabledTextColor;
    }



    private void OnContinue()
    {
        if (busy || !continueAvailable) return;
        onContinue.Invoke();
        
        if (onContinue.GetPersistentEventCount() == 0) StartCoroutine(FadeAndLoad());
    }

    private void OnNewGame()
    {
        if (busy) return;
        if (continueAvailable && confirmDialog != null)
        {
            confirmDialog.Show("Start a new game? Your current progress will be lost.", StartNewGame);
            return;
        }
        StartNewGame();
    }

    private void StartNewGame()
    {
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
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
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
        GameObject first = continueAvailable ? continueButton.gameObject : newGameButton.gameObject;
        EventSystem.current.SetSelectedGameObject(first);
    }

    private void SetMainMenuVisible(bool visible)
    {
        foreach (GameObject go in hideWhileSettingsOpen)
            if (go != null) go.SetActive(visible);
    }

    private IEnumerator FadeAndLoad()
    {
        busy = true;
        if (fadeToBlack != null)
        {
            fadeToBlack.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
                fadeToBlack.alpha = Mathf.Clamp01(t / fadeTime);
                yield return null;
            }
        }

        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.Log("TitleScreenUI: no gameplay scene name set yet, so nothing was loaded.");
            if (fadeToBlack != null) { fadeToBlack.alpha = 0f; fadeToBlack.blocksRaycasts = false; }
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
}
