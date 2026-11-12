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
    [SerializeField]
    private Color tabActiveColor =
        new Color32(0x5F, 0xE3, 0xE0, 0xFF);
    [SerializeField]
    private Color tabInactiveColor =
        new Color32(0xE8, 0xE1, 0xD0, 0xFF);

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

    [Header("Apply confirmation")]
    [SerializeField] private TMP_Text applyConfirmationText;
    [SerializeField, Min(0f)] private float confirmationDuration = 2f;

    [Header("Keys")]
    [SerializeField]
    private InputAction applyAction =
        new InputAction(
            "Apply",
            InputActionType.Button,
            "<Keyboard>/enter"
        );

    [SerializeField]
    private InputAction backAction =
        new InputAction(
            "Back",
            InputActionType.Button,
            "<Keyboard>/escape"
        );

    [Header("Teammate hook")]
    public UnityEvent onSettingsApplied = new UnityEvent();

    public bool IsOpen => root != null && root.activeSelf;

    private Action onBackCallback;
    private GameSettings.Values saved;
    private string savedBindingsJson = "";
    private bool dirty;
    private bool initialized;
    private float confirmationTimeLeft;

    private readonly List<Vector2Int> resolutions =
        new List<Vector2Int>();

    private void Awake()
    {
        Init();

        if (root != null && root != gameObject)
            root.SetActive(false);
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

        if (tabButtons != null)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int tab = i;

                if (tabButtons[i] != null)
                    tabButtons[i].onClick.AddListener(() => ShowTab(tab));
            }
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

        if (resolutionSelector != null)
        {
            resolutionSelector.onValueChanged.AddListener(
                _ => MarkDirty()
            );
        }

        if (qualitySelector != null)
        {
            qualitySelector.onValueChanged.AddListener(
                _ => MarkDirty()
            );
        }

        if (rebindRows != null)
        {
            foreach (RebindRow row in rebindRows)
            {
                if (row != null)
                    row.onChanged += MarkDirty;
            }
        }

        if (resetBindingsButton != null)
            resetBindingsButton.onClick.AddListener(ResetBindings);

        if (applyButton != null)
            applyButton.onClick.AddListener(Apply);

        if (backButton != null)
            backButton.onClick.AddListener(Back);

        BuildResolutionList();
        BuildQualityList();
        ClearApplyConfirmation();
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

        ClearApplyConfirmation();
    }

    private void Update()
    {
        if (confirmationTimeLeft <= 0f) return;

        confirmationTimeLeft -= Time.unscaledDeltaTime;

        if (confirmationTimeLeft <= 0f)
            ClearApplyConfirmation();
    }

    public void Open(Action onBack)
    {
        Init();

        if (root == null)
        {
            Debug.LogError(
                "SettingsMenuUI: assign the Root field.",
                this
            );
            return;
        }

        onBackCallback = onBack;
        saved = GameSettings.Load();

        savedBindingsJson = inputActions != null
            ? inputActions.SaveBindingOverridesAsJson()
            : "";

        WriteToUI(saved);
        RefreshRebindRows();

        dirty = false;
        ClearApplyConfirmation();

        root.SetActive(true);
        root.transform.SetAsLastSibling();

        ShowTab(0);
    }

    public void Apply()
    {
        if (RebindBusy()) return;

        if (confirmDialog != null && confirmDialog.IsOpen)
            return;

        saved = ReadFromUI();

        GameSettings.Save(saved);
        GameSettings.SaveBindings(inputActions);

        savedBindingsJson = inputActions != null
            ? inputActions.SaveBindingOverridesAsJson()
            : "";

        GameSettings.ApplyAudio(mixer, saved);
        GameSettings.ApplyVideo(saved, brightVolume, darkVolume);

        dirty = false;

        ShowApplyConfirmation();
        onSettingsApplied.Invoke();
    }

    public void Back()
    {
        if (RebindBusy()) return;

        if (dirty && confirmDialog != null)
        {
            confirmDialog.Show(
                "Discard changes?",
                () =>
                {
                    RevertPreview();
                    Close();
                }
            );

            return;
        }

        if (dirty)
            RevertPreview();

        Close();
    }

    private void Close()
    {
        ClearApplyConfirmation();

        if (root != null)
            root.SetActive(false);

        Action callback = onBackCallback;
        onBackCallback = null;
        callback?.Invoke();
    }

    private void RevertPreview()
    {
        GameSettings.ApplyAudio(mixer, saved);

        GameSettings.ApplyBrightness(
            saved.brightness,
            brightVolume,
            darkVolume
        );

        if (inputActions != null)
        {
            inputActions.RemoveAllBindingOverrides();

            if (!string.IsNullOrEmpty(savedBindingsJson))
            {
                inputActions.LoadBindingOverridesFromJson(
                    savedBindingsJson
                );
            }
        }

        RefreshRebindRows();

        dirty = false;
        ClearApplyConfirmation();
    }

    private void ResetBindings()
    {
        if (inputActions == null || RebindRow.IsAnyRebinding)
            return;

        inputActions.RemoveAllBindingOverrides();
        RefreshRebindRows();
        MarkDirty();
    }

    private void RefreshRebindRows()
    {
        if (rebindRows == null) return;

        foreach (RebindRow row in rebindRows)
        {
            if (row != null)
                row.Refresh();
        }
    }

    private static bool RebindBusy()
    {
        return RebindRow.IsAnyRebinding ||
               Time.frameCount <= RebindRow.LastRebindEndFrame + 1;
    }

    private void OnApplyKey(InputAction.CallbackContext ctx)
    {
        if (!IsOpen || RebindBusy())
            return;

        if (confirmDialog != null && confirmDialog.IsOpen)
            return;

        GameObject selected = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;

        if (selected != null && selected.GetComponent<Button>() != null)
            return;

        Apply();
    }

    private void OnBackKey(InputAction.CallbackContext ctx)
    {
        if (!IsOpen || RebindBusy())
            return;

        if (confirmDialog != null && confirmDialog.IsOpen)
        {
            confirmDialog.Close(false);
            return;
        }

        Back();
    }

    private void ShowTab(int index)
    {
        if (tabPages != null)
        {
            for (int i = 0; i < tabPages.Length; i++)
            {
                if (tabPages[i] != null)
                    tabPages[i].SetActive(i == index);
            }
        }

        if (tabButtons != null)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] == null) continue;

                TMP_Text label =
                    tabButtons[i].GetComponentInChildren<TMP_Text>();

                if (label != null)
                {
                    label.color = i == index
                        ? tabActiveColor
                        : tabInactiveColor;
                }
            }
        }

        if (EventSystem.current != null &&
            tabButtons != null &&
            index >= 0 &&
            index < tabButtons.Length &&
            tabButtons[index] != null)
        {
            EventSystem.current.SetSelectedGameObject(
                tabButtons[index].gameObject
            );
        }
    }

    private void WriteToUI(GameSettings.Values v)
    {
        SetSlider(masterSlider, masterValue, v.master, true);
        SetSlider(musicSlider, musicValue, v.music, true);
        SetSlider(sfxSlider, sfxValue, v.sfx, true);
        SetSlider(ambienceSlider, ambienceValue, v.ambience, true);

        SetSlider(
            brightnessSlider,
            brightnessValue,
            v.brightness,
            true
        );

        SetSlider(
            mouseSensSlider,
            mouseSensValue,
            v.mouseSens,
            false
        );

        SetToggle(fullscreenToggle, v.fullscreen);
        SetToggle(vsyncToggle, v.vsync);
        SetToggle(invertYToggle, v.invertY);
        SetToggle(cameraShakeToggle, v.cameraShake);
        SetToggle(enemyBarsToggle, v.enemyBars);
        SetToggle(subtitlesToggle, v.subtitles);

        if (resolutionSelector != null)
        {
            resolutionSelector.SetIndexWithoutNotify(
                FindResolutionIndex(v.resW, v.resH)
            );
        }

        if (qualitySelector != null)
            qualitySelector.SetIndexWithoutNotify(v.quality);
    }

    private GameSettings.Values ReadFromUI()
    {
        GameSettings.Values v = saved;

        if (masterSlider != null)
            v.master = masterSlider.value;

        if (musicSlider != null)
            v.music = musicSlider.value;

        if (sfxSlider != null)
            v.sfx = sfxSlider.value;

        if (ambienceSlider != null)
            v.ambience = ambienceSlider.value;

        if (brightnessSlider != null)
            v.brightness = brightnessSlider.value;

        if (mouseSensSlider != null)
            v.mouseSens = mouseSensSlider.value;

        if (fullscreenToggle != null)
            v.fullscreen = fullscreenToggle.isOn;

        if (vsyncToggle != null)
            v.vsync = vsyncToggle.isOn;

        if (invertYToggle != null)
            v.invertY = invertYToggle.isOn;

        if (cameraShakeToggle != null)
            v.cameraShake = cameraShakeToggle.isOn;

        if (enemyBarsToggle != null)
            v.enemyBars = enemyBarsToggle.isOn;

        if (subtitlesToggle != null)
            v.subtitles = subtitlesToggle.isOn;

        if (resolutionSelector != null && resolutions.Count > 0)
        {
            int selectedIndex = Mathf.Clamp(
                resolutionSelector.Index,
                0,
                resolutions.Count - 1
            );

            Vector2Int resolution = resolutions[selectedIndex];
            v.resW = resolution.x;
            v.resH = resolution.y;
        }

        if (qualitySelector != null)
            v.quality = qualitySelector.Index;

        return v;
    }

    private void PreviewLive()
    {
        GameSettings.Values v = ReadFromUI();

        GameSettings.ApplyAudio(mixer, v);

        GameSettings.ApplyBrightness(
            v.brightness,
            brightVolume,
            darkVolume
        );
    }

    private void BuildResolutionList()
    {
        resolutions.Clear();

        foreach (Resolution resolution in Screen.resolutions)
        {
            Vector2Int size = new Vector2Int(
                resolution.width,
                resolution.height
            );

            if (!resolutions.Contains(size))
                resolutions.Add(size);
        }

        resolutions.Sort(
            (a, b) => a.x != b.x
                ? a.x.CompareTo(b.x)
                : a.y.CompareTo(b.y)
        );

        if (resolutions.Count == 0)
        {
            resolutions.Add(
                new Vector2Int(Screen.width, Screen.height)
            );
        }

        List<string> labels = new List<string>();

        foreach (Vector2Int resolution in resolutions)
        {
            labels.Add(
                resolution.x + " x " + resolution.y
            );
        }

        if (resolutionSelector != null)
        {
            resolutionSelector.SetOptions(
                labels,
                resolutions.Count - 1
            );
        }
    }

    private void BuildQualityList()
    {
        List<string> names = new List<string>();

        foreach (string qualityName in QualitySettings.names)
            names.Add(qualityName.ToUpperInvariant());

        if (qualitySelector != null)
        {
            qualitySelector.SetOptions(
                names,
                QualitySettings.GetQualityLevel()
            );
        }
    }

    private int FindResolutionIndex(int width, int height)
    {
        int index = resolutions.IndexOf(
            new Vector2Int(width, height)
        );

        if (index >= 0)
            return index;

        index = resolutions.IndexOf(
            new Vector2Int(Screen.width, Screen.height)
        );

        return index >= 0 ? index : resolutions.Count - 1;
    }

    private void MarkDirty()
    {
        dirty = true;
        ClearApplyConfirmation();
    }

    private void ShowApplyConfirmation()
    {
        if (applyConfirmationText == null) return;

        applyConfirmationText.text = "Settings applied";
        applyConfirmationText.color =
            new Color32(0x5F, 0xE3, 0xE0, 0xFF);

        applyConfirmationText.raycastTarget = false;
        applyConfirmationText.gameObject.SetActive(true);

        confirmationTimeLeft = Mathf.Max(
            0.1f,
            confirmationDuration
        );
    }

    private void ClearApplyConfirmation()
    {
        confirmationTimeLeft = 0f;

        if (applyConfirmationText != null)
            applyConfirmationText.gameObject.SetActive(false);
    }

    private static void SetupSlider(
        Slider slider,
        float min,
        float max,
        bool whole)
    {
        if (slider == null) return;

        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = whole;
    }

    private void AddSlider(
        Slider slider,
        TMP_Text label,
        bool whole)
    {
        if (slider == null) return;

        slider.onValueChanged.AddListener(value =>
        {
            UpdateLabel(label, value, whole);
            MarkDirty();
            PreviewLive();
        });

        EventTrigger trigger = s.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = s.gameObject.AddComponent<EventTrigger>();
        }

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener(_ => PlaySfx(clickSfx));
        trigger.triggers.Add(entry);
    }

    private void AddToggle(Toggle toggle)
    {
        if (toggle == null) return;

        toggle.onValueChanged.AddListener(
            _ => MarkDirty()
        );
    }

    private static void SetSlider(
        Slider slider,
        TMP_Text label,
        float value,
        bool whole)
    {
        if (slider == null) return;

        slider.SetValueWithoutNotify(value);
        UpdateLabel(label, value, whole);
    }

    private static void SetToggle(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(value);
    }

    private static void UpdateLabel(
        TMP_Text label,
        float value,
        bool whole)
    {
        if (label == null) return;

        label.text = whole
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.0");
    }

    private void PlaySfx(string Name)
    {
        if (AudioManger.inst == null)
        {
            return;
        }
        AudioManger.inst.PlaySFX(Name);
    }
    


}