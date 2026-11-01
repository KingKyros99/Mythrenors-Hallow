using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class OptionSelector : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TMP_Text valueLabel;
    [SerializeField] private bool wrapAround = false;

    public UnityEvent<int> onValueChanged = new UnityEvent<int>();

    private readonly List<string> options = new List<string>();
    public int Index { get; private set; }

    private void Awake()
    {
        if (leftButton != null) leftButton.onClick.AddListener(() => Step(-1));
        if (rightButton != null) rightButton.onClick.AddListener(() => Step(1));
    }

    public void SetOptions(List<string> newOptions, int startIndex)
    {
        options.Clear();
        options.AddRange(newOptions);
        SetIndexWithoutNotify(startIndex);
    }

    public void SetIndexWithoutNotify(int index)
    {
        if (options.Count == 0) { Index = 0; Refresh(); return; }
        Index = Mathf.Clamp(index, 0, options.Count - 1);
        Refresh();
    }

    public void Step(int direction)
    {
        if (options.Count == 0) return;
        int next = Index + direction;
        if (wrapAround) next = (next + options.Count) % options.Count;
        next = Mathf.Clamp(next, 0, options.Count - 1);
        if (next == Index) return;
        Index = next;
        Refresh();
        onValueChanged.Invoke(Index);
    }

    private void Refresh()
    {
        if (valueLabel != null)
            valueLabel.text = options.Count > 0 ? options[Index] : "-";
        if (!wrapAround)
        {
            if (leftButton != null) leftButton.interactable = Index > 0;
            if (rightButton != null) rightButton.interactable = Index < options.Count - 1;
        }
    }
}
