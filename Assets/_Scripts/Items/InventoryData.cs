using UnityEngine;

[System.Serializable]
public class InventoryData
{
    public ItemData itemData;
    public int quantity;

    public InventoryData(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
    }
}