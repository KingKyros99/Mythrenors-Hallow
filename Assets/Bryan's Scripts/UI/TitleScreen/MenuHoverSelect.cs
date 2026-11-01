using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuHoverSelect : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private TitleMenuSelector selector;
    [SerializeField] private int index;

    public void OnPointerEnter(PointerEventData e)
    {
        var b = GetComponent<Button>();
        if (b != null && b.interactable) selector.Select(index);
    }
}