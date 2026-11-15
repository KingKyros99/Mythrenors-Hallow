using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private ItemSlot itemSlotPrefab;

    private bool isOpen;
    private readonly List<ItemSlot> slots = new List<ItemSlot>();

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        RefreshInventory();
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
            ToggleInventory();
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isOpen);
    }         

    private void RefreshInventory()
    {
        Debug.Log($"REFRESH FIRED - Inventory entries: {InventoryManager.Instance.Items.Count}");

        if (InventoryManager.Instance == null || itemGrid == null || itemSlotPrefab == null)
            return;

        foreach (ItemSlot slot in slots)
            Destroy(slot.gameObject);

        slots.Clear();

        foreach (InventoryData item in InventoryManager.Instance.Items)
        {
            ItemSlot slot = Instantiate(itemSlotPrefab, itemGrid);
            slot.Setup(item);
            Debug.Log($"CREATED SLOT: {item.itemData.itemName} x{item.quantity}");
            slots.Add(slot);
        }
    }
}
