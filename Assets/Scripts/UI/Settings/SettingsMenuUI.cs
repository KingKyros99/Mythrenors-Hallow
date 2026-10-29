using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject root;              
    [SerializeField] private ConfirmDialog confirmDialog;  

    [Header("Tabs (same order in both lists)")]
    [SerializeField] private Button[] tabButtons;          
    [SerializeField] private GameObject[] tabPages;       
    [SerializeField] private Color tabActiveColor = new Color32(0x5F, 0xE3, 0xE0, 0xFF);
    [SerializeField] private Color tabInactiveColor = new Color32(0xE8, 0xE1, 0xD0, 0xFF);

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambienceSlider;
    [SerializeField] private TMP_Text masterValue;
    [SerializeField] private TMP_Text musicValue;
    [SerializeField] private TMP_Text sfxValue;
    [SerializeField] private TMP_Text ambienceValue;

    [Header("Video")]
    [SerializeField] private OptionSelector resolutionSelector;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private OptionSelector qualitySelector;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Text brightnessValue;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private Volume brightVolume;
    [SerializeField] private Volume darkVolume;

    [Header("Controls")]
    [SerializeField] private Slider mouseSensSlider;
    [SerializeField] private TMP_Text mouseSensValue;
    [SerializeField] private Toggle invertYToggle;

    [Header("Key Bindings (keyboard and mouse)")]
    [SerializeField] private InputActionAsset inputActions;  
    [SerializeField] private RebindRow[] rebindRows;
    [SerializeField] private Button resetBindingsButton;

    [Header("Gameplay (optional - leave empty to skip)")]
    [SerializeField] private Toggle cameraShakeToggle;
    [SerializeField] private Toggle enemyBarsToggle;
    [SerializeField] private Toggle subtitlesToggle;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button backButton;

    [Header("Keys")]
    [SerializeField] private InputAction applyAction = new InputAction("Apply", InputActionType.Button, "<Keyboard>/enter");
    [SerializeField] private InputAction backAction = new InputAction("Back", InputActionType.Button, "<Keyboard>/escape");

    [Header("Teammate hook")]
    public UnityEvent onSettingsApplied = new UnityEvent();

    public bool IsOpen => root != null && root.activeSelf;

    private Action onBackCallback;
    private GameSettings.Values saved;
    private string savedBindingsJson = "";
    private bool dirty;
    private readonly List<Vector2Int> resolutions = new List<Vector2Int>();

    private bool initialized;

    private void Awake()
    {
        Init();
        if (root != null && root != gameObject) root.SetActive(false);
    }

    private void Init()
    {
        if (initialized) return;
        initialized = true;

        
        SetupSlider(masterSlider, 0, 100, true);
        SetupSlider(musicSlider, 0, 100, true);
        SetupSlider(sfxSlider, 0, 100, true);
        SetupSlider(ambienceSlider, 0, 100, true);
        SetupSlider(brightnessSlider, 0, 100, true);
        SetupSlider(mouseSensSlider, 0.1f, 3f, false);

        
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int tab = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(tab));
        }

        
        AddSlider(masterSlider, masterValue, true);
        AddSlider(musicSlider, musicValue, true);
        AddSlider(sfxSlider, sfxValue, true);
        AddSlider(ambienceSlider, ambienceValue, true);
        AddSlider(brightnessSlider, brightnessValue, true);
        AddSlider(mouseSensSlider, mouseSensValue, false);
        AddToggle(fullscreenToggle);
        AddToggle(vsyncToggle);
        AddToggle(invertYToggle);
        AddToggle(cameraShakeToggle);
        AddToggle(enemyBarsToggle);
        AddToggle(subtitlesToggle);
        if (resolutionSelector != null) resolutionSelector.onValueChanged.AddListener(_ => dirty = true);
        if (qualitySelector != null) qualitySelector.onValueChanged.AddListener(_ => dirty = true);

        foreach (RebindRow row in rebindRows)
            if (row != null) row.onChanged += () => dirty = true;
        if (resetBindingsButton != null) resetBindingsButton.onClick.AddListener(ResetBindings);

        applyButton.onClick.AddListener(Apply);
        backButton.onClick.AddListener(Back);

        BuildResolutionList();
        BuildQualityList();
    }

    private void OnEnable()
    {
        applyAction.Enable();
        backAction.Enable();
        applyAction.performed += OnApplyKey;
        backAction.performed += OnBackKey;
    }

    private void OnDisable()
    {
        applyAction.performed -= OnApplyKey;
        backAction.performed -= OnBackKey;
        applyAction.Disable();
        backAction.Disable();
    }

    
    public void Open(Action onBack)
    {
        Init();
        onBackCallback = onBack;
        saved = GameSettings.Load();
        savedBindingsJson = inputActions != null ? inputActions.SaveBindingOverridesAsJson() : "";
        WriteToUI(saved);
        RefreshRebindRows();
        dirty = false;
        root.SetActive(true);
        root.transform.SetAsLastSibling();
        ShowTab(0);
    }

    public void Apply()
    {
        saved = ReadFromUI();
        GameSettings.Save(saved);
        GameSettings.SaveBindings(inputActions);
        savedBindingsJson = inputActions != null ? inputActions.SaveBindingOverridesAsJson() : "";
        GameSettings.ApplyAudio(mixer, saved);
        GameSettings.ApplyVideo(saved, brightVolume, darkVolume);
        dirty = false;
        onSettingsApplied.Invoke();
    }

    public void Back()
    {
        if (RebindBusy()) return;
        if (dirty && confirmDialog != null)
        {
            confirmDialog.Show("Discard changes?", () => { RevertPreview(); Close(); });
            return;
        }
        if (dirty) RevertPreview();
        Close();
    }
        private void Close()
    {
        root.SetActive(false);
        Action callback = onBackCallback;
        onBackCallback = null;
        callback?.Invoke();
    }

    private void RevertPreview()
    {
       
        GameSettings.ApplyAudio(mixer, saved);
        GameSettings.ApplyBrightness(saved.brightness, brightVolume, darkVolume);
        if (inputActions != null)
        {
            inputActions.RemoveAllBindingOverrides();
            if (!string.IsNullOrEmpty(savedBindingsJson)) inputActions.LoadBindingOverridesFromJson(savedBindingsJson);
        }
        RefreshRebindRows();
        dirty = false;
    }

    private void ResetBindings()
    {
        if (inputActions == null || RebindRow.IsAnyRebinding) return;
        inputActions.RemoveAllBindingOverrides();
        RefreshRebindRows();
        dirty = true;
    }

    private void RefreshRebindRows()
    {
        foreach (RebindRow row in rebindRows)
            if (row != null) row.Refresh();
    }

    
    private static bool RebindBusy()
    {
        return RebindRow.IsAnyRebinding || Time.frameCount <= RebindRow.LastRebindEndFrame + 1;
    }

    private void OnApplyKey(InputAction.CallbackContext ctx)
    {
        if (!IsOpen || RebindBusy() || (confirmDialog != null && confirmDialog.IsOpen)) return;
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (selected != null && selected.GetComponent<Button>() != null) return;
        Apply();
    }

    private void OnBackKey(InputAction.CallbackContext ctx)
    {
        if (!IsOpen || RebindBusy()) return;
        if (confirmDialog != null && confirmDialog.IsOpen) { confirmDialog.Close(false); return; }
        Back();
    }

    private void ShowTab(int index)
    {
        for (int i = 0; i < tabPages.Length; i++)
            if (tabPages[i] != null) tabPages[i].SetActive(i == index);

        for (int i = 0; i < tabButtons.Length; i++)
        {
            TMP_Text label = tabButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = i == index ? tabActiveColor : tabInactiveColor;
        }

        if (EventSystem.current != null && index < tabButtons.Length)
            EventSystem.current.SetSelectedGameObject(tabButtons[index].gameObject);
    }

    private void WriteToUI(GameSettings.Values v)
    {
        SetSlider(masterSlider, masterValue, v.master, true);
        SetSlider(musicSlider, musicValue, v.music, true);
        SetSlider(sfxSlider, sfxValue, v.sfx, true);
        SetSlider(ambienceSlider, ambienceValue, v.ambience, true);
        SetSlider(brightnessSlider, brightnessValue, v.brightness, true);
        SetSlider(mouseSensSlider, mouseSensValue, v.mouseSens, false);

        SetToggle(fullscreenToggle, v.fullscreen);
        SetToggle(vsyncToggle, v.vsync);
        SetToggle(invertYToggle, v.invertY);
        SetToggle(cameraShakeToggle, v.cameraShake);
        SetToggle(enemyBarsToggle, v.enemyBars);
        SetToggle(subtitlesToggle, v.subtitles);

        if (resolutionSelector != null) resolutionSelector.SetIndexWithoutNotify(FindResolutionIndex(v.resW, v.resH));
        if (qualitySelector != null) qualitySelector.SetIndexWithoutNotify(v.quality);
    }

    private GameSettings.Values ReadFromUI()
    {
        GameSettings.Values v = saved;
        if (masterSlider) v.master = masterSlider.value;
        if (musicSlider) v.music = musicSlider.value;
        if (sfxSlider) v.sfx = sfxSlider.value;
        if (ambienceSlider) v.ambience = ambienceSlider.value;
        if (brightnessSlider) v.brightness = brightnessSlider.value;
        if (mouseSensSlider) v.mouseSens = mouseSensSlider.value;
        if (fullscreenToggle) v.fullscreen = fullscreenToggle.isOn;
        if (vsyncToggle) v.vsync = vsyncToggle.isOn;
        if (invertYToggle) v.invertY = invertYToggle.isOn;
        if (cameraShakeToggle) v.cameraShake = cameraShakeToggle.isOn;
        if (enemyBarsToggle) v.enemyBars = enemyBarsToggle.isOn;
        if (subtitlesToggle) v.subtitles = subtitlesToggle.isOn;
        if (resolutionSelector != null && resolutions.Count > 0)
        {
            Vector2Int r = resolutions[resolutionSelector.Index];
            v.resW = r.x;
            v.resH = r.y;
        }
        if (qualitySelector != null) v.quality = qualitySelector.Index;
        return v;
    }

    private void PreviewLive()
    {
        GameSettings.Values v = ReadFromUI();
        GameSettings.ApplyAudio(mixer, v);
        GameSettings.ApplyBrightness(v.brightness, brightVolume, darkVolume);
    }

    private void BuildResolutionList()
    {
        resolutions.Clear();
        foreach (Resolution r in Screen.resolutions)
        {
            Vector2Int size = new Vector2Int(r.width, r.height);
            if (!resolutions.Contains(size)) resolutions.Add(size);
        }
        resolutions.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        if (resolutions.Count == 0) resolutions.Add(new Vector2Int(Screen.width, Screen.height));

        List<string> labels = new List<string>();
        foreach (Vector2Int r in resolutions) labels.Add(r.x + " x " + r.y);
        if (resolutionSelector != null) resolutionSelector.SetOptions(labels, resolutions.Count - 1);
    }

    private void BuildQualityList()
    {
        List<string> names = new List<string>();
        foreach (string n in QualitySettings.names) names.Add(n.ToUpperInvariant());
        if (qualitySelector != null) qualitySelector.SetOptions(names, QualitySettings.GetQualityLevel());
    }

    private int FindResolutionIndex(int w, int h)
    {
        int index = resolutions.IndexOf(new Vector2Int(w, h));
        if (index >= 0) return index;
        index = resolutions.IndexOf(new Vector2Int(Screen.width, Screen.height));
        return index >= 0 ? index : resolutions.Count - 1;
    }

    

    private static void SetupSlider(Slider s, float min, float max, bool whole)
    {
        if (s == null) return;
        s.minValue = min;
        s.maxValue = max;
        s.wholeNumbers = whole;
    }

    private void AddSlider(Slider s, TMP_Text label, bool whole)
    {
        if (s == null) return;
        s.onValueChanged.AddListener(value =>
        {
            UpdateLabel(label, value, whole);
            dirty = true;
            PreviewLive();
        });
    }

    private void AddToggle(Toggle t)
    {
        if (t == null) return;
        t.onValueChanged.AddListener(_ => dirty = true);
    }

    private static void SetSlider(Slider s, TMP_Text label, float value, bool whole)
    {
        if (s == null) return;
        s.SetValueWithoutNotify(value);
        UpdateLabel(label, value, whole);
    }

    private static void SetToggle(Toggle t, bool value)
    {
        if (t != null) t.SetIsOnWithoutNotify(value);
    }

    private static void UpdateLabel(TMP_Text label, float value, bool whole)
    {
        if (label != null) label.text = whole ? Mathf.RoundToInt(value).ToString() : value.ToString("0.0");
    }
}
