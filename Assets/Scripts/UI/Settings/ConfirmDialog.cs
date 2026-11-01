using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ConfirmDialog : MonoBehaviour
{
    [SerializeField] private GameObject root;     
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action yesAction;
    private Action noAction;
    private GameObject previousSelection;

    public bool IsOpen => root != null && root.activeSelf;

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
        yesButton.onClick.AddListener(() => Close(true));
        noButton.onClick.AddListener(() => Close(false));
    }

    public void Show(string message, Action onYes, Action onNo = null)
    {
        Init();
        yesAction = onYes;
        noAction = onNo;
        if (messageText != null) messageText.text = message;
        previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        root.SetActive(true);
        root.transform.SetAsLastSibling();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(noButton.gameObject);
    }

    public void Close(bool yes)
    {
        root.SetActive(false);
        Action action = yes ? yesAction : noAction;
        yesAction = null;
        noAction = null;
        if (!yes && previousSelection != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(previousSelection);
        action?.Invoke();
    }
}
