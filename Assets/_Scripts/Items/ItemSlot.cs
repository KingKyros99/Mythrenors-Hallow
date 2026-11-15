using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public TMP_Text nameText;
    public Image iconImage;
    public TMP_Text quantityText;

    private InventoryData item;

    public InventoryData CurrentItem => item;

    public void Setup(InventoryData newItem)
    {
        item = newItem;

        if (item == null || item.itemData == null)
        {
            Clear();
            return;
        }

        if (nameText != null)
            nameText.text = "";

        if (iconImage != null)
        {
            iconImage.sprite = item.itemData.icon;
            iconImage.enabled = item.itemData.icon != null;
        }

        if (quantityText != null)
        {
            quantityText.text = item.quantity.ToString();
            quantityText.enabled = true;
            //Debug.Log($"DISPLAYING QUANTITY: {quantityText.text}");
        }

        //Debug.Log($"SLOT SETUP: {item.itemData.itemName} x{item.quantity}");
    }

    public void Clear()
    {
        item = null;

        if (nameText != null)
            nameText.text = "";

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (quantityText != null)
            quantityText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null)
            return;

        InventoryUIController ui =
            Object.FindAnyObjectByType<InventoryUIController>();

        //if (ui != null)
        //    ui.ShowItemDetails(item);
    }
}
