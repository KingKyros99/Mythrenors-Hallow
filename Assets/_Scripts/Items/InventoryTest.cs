using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryTest : MonoBehaviour
{
    [SerializeField] private ItemData testPotion;

    private void Update()
    {
        if (Keyboard.current == null)
        {
            //Debug.LogWarning("Keyboard.current is NULL");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            //Debug.LogWarning("InventoryManager.Instance is NULL");
            return;
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            //Debug.Log("P DETECTED!");

            bool added = InventoryManager.Instance.AddItem(testPotion);

            //Debug.Log($"ADD RESULT: {added}");
            //Debug.Log($"POTION COUNT: {GetPotionCount()}");
        }

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            //Debug.Log("O DETECTED!");

            bool removed = InventoryManager.Instance.RemoveItem(testPotion);

            //Debug.Log($"REMOVE RESULT: {removed}");
            //Debug.Log($"POTION COUNT: {GetPotionCount()}");
        }
    }

    private int GetPotionCount()
    {
        foreach (InventoryData item in InventoryManager.Instance.Items)
        {
            if (item.itemData == testPotion)
                return item.quantity;
        }

        return 0;
    }
}

