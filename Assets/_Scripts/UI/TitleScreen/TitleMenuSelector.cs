using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleMenuSelector : MonoBehaviour
{
    [SerializeField] private RectTransform crystal;
    [SerializeField] private RectTransform line;
    [SerializeField] private Button[] buttons;
    [SerializeField] private float slideSpeed = 14f;
    [SerializeField] private float lineOffsetY = -20f;

    [SerializeField] private string clickSfx;
    [SerializeField] private string hoverSfx;


    private int index = -1;
    private GameObject lastSelected;

    private void OnEnable() { SelectFirst(); }

    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].onClick.AddListener(ClickSound);
            }
        }
    }

    public void SelectFirst()
    {
        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i] != null && buttons[i].interactable) { Select(i); return; }
    }

    public void Select(int i)
    {
        if (i < 0 || i >= buttons.Length || !buttons[i].interactable) return;
        index = i;

        if (AudioManger.inst != null)
        {
            AudioManger.inst.PlaySFX(hoverSfx);
        }
    }
    private void ClickSound()
    {
        if (AudioManger.inst != null)
        {
            AudioManger.inst.PlaySFX(clickSfx);
        }
    }
    private void Update()
    {
        FollowKeyboardSelection();

        if (index < 0 || index >= buttons.Length) return;
        Vector3 p = buttons[index].transform.position;
        float t = 1f - Mathf.Exp(-slideSpeed * Time.unscaledDeltaTime);
        if (crystal != null)
            crystal.position = new Vector3(crystal.position.x, Mathf.Lerp(crystal.position.y, p.y, t), crystal.position.z);
        if (line != null)
            line.position = new Vector3(line.position.x, Mathf.Lerp(line.position.y, p.y + lineOffsetY, t), line.position.z);
    }

    private void FollowKeyboardSelection()
    {
        if (EventSystem.current == null) return;
        GameObject sel = EventSystem.current.currentSelectedGameObject;
        if (sel == lastSelected) return;
        lastSelected = sel;
        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i] != null && buttons[i].gameObject == sel) { Select(i); return; }
    }
}