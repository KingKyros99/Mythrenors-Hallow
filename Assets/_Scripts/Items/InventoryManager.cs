using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private readonly List<InventoryData> items = new List<InventoryData>();

    public event Action OnInventoryChanged;

    public IReadOnlyList<InventoryData> Items => items;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddItem(ItemData data, int amount = 1)
    {
        if (data == null || amount <= 0)
            return false;

        // Consumables only for now.
        if (data.category != ItemCategory.CONSUMABLE)
            return false;

        InventoryData existing = items.Find(
            item => item.itemData == data
        );

        if (existing != null)
        {
            existing.quantity += amount;
        }
        else
        {
            items.Add(new InventoryData(data, amount));
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemData data, int amount = 1)
    {
        if (data == null || amount <= 0)
            return false;

        InventoryData existing = items.Find(
            item => item.itemData == data
        );

        if (existing == null || existing.quantity < amount)
            return false;

        existing.quantity -= amount;

        if (existing.quantity <= 0)
            items.Remove(existing);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(ItemData data, int amount = 1)
    {
        if (data == null || amount <= 0)
            return false;

        InventoryData existing = items.Find(
            item => item.itemData == data
        );

        return existing != null && existing.quantity >= amount;
    }

    public void ClearInventory()
    {
        if (items.Count == 0)
            return;

        items.Clear();
        OnInventoryChanged?.Invoke();
    }
}
