using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerMoveHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        ColorBlock colors = button.colors;
        colors.highlightedColor = colors.normalColor;
        button.colors = colors;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectButton();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (eventData.delta.sqrMagnitude > 0f)
            SelectButton();
    }

    private void SelectButton()
    {
        if (!button.IsActive() || !button.IsInteractable())
            return;

        EventSystem events = EventSystem.current;

        if (events == null || events.alreadySelecting)
            return;

        if (events.currentSelectedGameObject != gameObject)
            events.SetSelectedGameObject(gameObject);
    }
}