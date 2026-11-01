using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class RebindRow : MonoBehaviour
{
    [Header("What this row changes")]
    [SerializeField] private InputActionReference action;   
    [Tooltip("Leave empty for normal actions. For Move, use up, down, left or right.")]
    [SerializeField] private string compositePart = "";

    [Header("UI")]
    [SerializeField] private TMP_Text actionLabel;          
    [SerializeField] private string displayName = "";
    [SerializeField] private Button keyButton;              
    [SerializeField] private TMP_Text keyLabel;             
    [SerializeField] private string waitingText = "PRESS A KEY...";
    [SerializeField] private string inUseText = "ALREADY USED";

    public static bool IsAnyRebinding { get; private set; }
    public static int LastRebindEndFrame { get; private set; } = -10;

    public event Action onChanged;

    private InputActionRebindingExtensions.RebindingOperation operation;

    private void Awake()
    {
        if (keyButton != null) keyButton.onClick.AddListener(StartRebind);
        if (actionLabel != null && !string.IsNullOrEmpty(displayName)) actionLabel.text = displayName;
    }

    private void OnEnable() { Refresh(); }

    private void OnDisable()
    {
        if (operation != null) operation.Cancel();
    }

    public void Refresh()
    {
        if (keyLabel == null || action == null || action.action == null) return;
        int index = FindBindingIndex();
        keyLabel.text = index < 0
            ? "-"
            : action.action.GetBindingDisplayString(index, InputBinding.DisplayStringOptions.DontIncludeInteractions).ToUpperInvariant();
    }

    private int FindBindingIndex()
    {
        InputAction a = action.action;
        for (int i = 0; i < a.bindings.Count; i++)
        {
            InputBinding b = a.bindings[i];
            if (b.isComposite) continue;
            bool isKeyboardOrMouse = b.path.StartsWith("<Keyboard>") || b.path.StartsWith("<Mouse>");
            if (!isKeyboardOrMouse) continue;

            if (string.IsNullOrEmpty(compositePart))
            {
                if (!b.isPartOfComposite) return i;
            }
            else if (b.isPartOfComposite && string.Equals(b.name, compositePart, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    private void StartRebind()
    {
        if (IsAnyRebinding || Time.frameCount <= LastRebindEndFrame + 1) return;
        if (action == null || action.action == null) return;
        int index = FindBindingIndex();
        if (index < 0)
        {
            Debug.LogWarning($"RebindRow: no keyboard/mouse binding found on {action.action.name} {compositePart}");
            return;
        }

        InputAction a = action.action;
        bool wasEnabled = a.enabled;
        string previousOverride = a.bindings[index].overridePath;

        a.Disable();                     
        IsAnyRebinding = true;
        keyLabel.text = waitingText;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null); 

        operation = a.PerformInteractiveRebinding(index)
            .WithControlsHavingToMatchPath("<Keyboard>")
            .WithControlsHavingToMatchPath("<Mouse>")
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Mouse>/scroll")
            .WithControlsExcluding("<Mouse>/press")
            .WithControlsExcluding("<Keyboard>/anyKey")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                bool duplicate = IsUsedElsewhere(a, index);
                if (duplicate)
                {
                    if (string.IsNullOrEmpty(previousOverride)) a.RemoveBindingOverride(index);
                    else a.ApplyBindingOverride(index, previousOverride);
                }
                Finish(a, wasEnabled);
                if (duplicate) StartCoroutine(ShowInUse());
                else onChanged?.Invoke();
            })
            .OnCancel(op => Finish(a, wasEnabled));

        operation.Start();
    }

    private void Finish(InputAction a, bool wasEnabled)
    {
        operation?.Dispose();
        operation = null;
        IsAnyRebinding = false;
        LastRebindEndFrame = Time.frameCount;
        if (wasEnabled) a.Enable();
        Refresh();
        if (EventSystem.current != null && keyButton != null) EventSystem.current.SetSelectedGameObject(keyButton.gameObject);
    }

    
    private static bool IsUsedElsewhere(InputAction a, int index)
    {
        string newPath = a.bindings[index].effectivePath;
        if (string.IsNullOrEmpty(newPath) || a.actionMap == null) return false;

        foreach (InputAction other in a.actionMap.actions)
        {
            for (int i = 0; i < other.bindings.Count; i++)
            {
                if (other == a && i == index) continue;
                InputBinding b = other.bindings[i];
                if (b.isComposite) continue;
                if (string.Equals(b.effectivePath, newPath, StringComparison.OrdinalIgnoreCase)) return true;
            }
        }
        return false;
    }

    private IEnumerator ShowInUse()
    {
        if (keyLabel != null) keyLabel.text = inUseText;
        yield return new WaitForSecondsRealtime(1f);
        Refresh();
    }
}
