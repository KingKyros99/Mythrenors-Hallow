using UnityEngine;

public class ItemProvider : MonoBehaviour
{
    [Header("Available Items")]
    [SerializeField] private ItemData[] availableItems;

    public ItemData GetItem()
    {
        if (availableItems == null || availableItems.Length == 0)
        {
            //Debug.LogWarning("No items assigned to ItemProvider.");
            return null;
        }

        int randomIndex = Random.Range(0, availableItems.Length);

        return availableItems[randomIndex];
    }

    public ItemData GetRandomItem()
    {
        if (availableItems == null || availableItems.Length == 0)
        {
            //Debug.LogWarning("No items assigned to ItemProvider.");
            return null;
        }

        // Count valid item entries.
        int validCount = 0;

        foreach (ItemData item in availableItems)
        {
            if (item != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            //Debug.LogWarning("ItemProvider contains no valid items.");
            return null;
        }

        // Pick a random valid item.
        int randomIndex = Random.Range(0, validCount);

        foreach (ItemData item in availableItems)
        {
            if (item == null)
                continue;

            if (randomIndex == 0)
                return item;

            randomIndex--;
        }

        return null;
    }

    //public void PopulateContainer(VirtualLootContainer container, int itemCount)
    //{
    //    if (container == null)
    //    {
    //        Debug.LogWarning("Cannot populate a null ItemContainer.");
    //        return;
    //    }

    //    for (int i = 0; i < itemCount; i++)
    //    {
    //        ItemData item = GetRandomItem();

    //        if (item == null)
    //            return;

    //        bool added = container.AddLoot(item);

    //        if (!added)
    //        {
    //            Debug.LogWarning("Could not add item to loot container: " + item.itemName);
    //            return;
    //        }
    //    }
    //}
}
