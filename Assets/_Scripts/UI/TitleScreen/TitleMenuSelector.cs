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
    private bool snapToSelection;

    private void OnEnable()
    {
        lastSelected = null;
        snapToSelection = true;
        SelectFirst();
    }

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
        index = -1;

        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (IsAvailable(buttons[i]))
            {
                Select(i);
                return;
            }
        }
    }

    public void Select(int i)
    {
        if (buttons == null ||
            i < 0 ||
            i >= buttons.Length ||
            !IsAvailable(buttons[i]))
        {
            return;
        }

        index = i;

        if (AudioManger.inst != null)
        {
            AudioManger.inst.PlaySFX(hoverSfx);
        }
    }

    private void LateUpdate()
    {
        FollowKeyboardSelection();

        if (buttons == null ||
            index < 0 ||
            index >= buttons.Length)
        {
            return;
        }

        if (!IsAvailable(buttons[index]))
        {
            SelectFirst();

            if (index < 0) return;
        }


        if (snapToSelection)
            Canvas.ForceUpdateCanvases();

        RectTransform buttonRect =
            buttons[index].transform as RectTransform;

        if (buttonRect == null) return;

        Vector3 buttonCenter =
            buttonRect.TransformPoint(buttonRect.rect.center);

        float blend = snapToSelection
            ? 1f
            : 1f - Mathf.Exp(
                -Mathf.Max(0f, slideSpeed) * Time.unscaledDeltaTime
            );

        MoveVertically(crystal, buttonCenter, 0f, blend);

        // Move this after the crystal because the line may be its child.
        if (line != null && line != crystal)
            MoveVertically(line, buttonCenter, lineOffsetY, blend);

        snapToSelection = false;
    }

    private void MoveVertically(
        RectTransform decoration,
        Vector3 worldTarget,
        float offsetY,
        float blend)
    {
        if (decoration == null || decoration.parent == null) return;

        Transform parent = decoration.parent;

        Vector3 target = parent.InverseTransformPoint(worldTarget);
        Vector3 position = decoration.localPosition;


        float targetY =
            target.y -
            decoration.rect.center.y * decoration.localScale.y +
            offsetY;

        position.y = Mathf.Lerp(position.y, targetY, blend);

        // this should Preserve the horizontal placement you set in the Inspector.
        decoration.localPosition = position;
    }

    private void FollowKeyboardSelection()
    {
        if (EventSystem.current == null || buttons == null) return;

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        if (selected == lastSelected) return;

        lastSelected = selected;

        if (selected == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null &&
                buttons[i].gameObject == selected)
            {
                Select(i);
                return;
            }
        }
    }

    private void ClickSound()
    {
        if (AudioManger.inst != null)
        {
            AudioManger.inst.PlaySFX(clickSfx);
        }
    }
    private bool IsAvailable(Button button)
    {
        return button != null &&
               button.isActiveAndEnabled &&
               button.gameObject.activeInHierarchy &&
               button.IsInteractable();
    }
}