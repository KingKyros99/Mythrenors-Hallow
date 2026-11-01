using System.Collections.Generic;
using UnityEngine;


public class Player : CharacterBase
{
    //[Header("Player Inventory")]
    //[SerializeField] private List<Item> inventory = new List<Item>();

    //public IReadOnlyList<Item> Inventory => inventory;

    [Header("Player Movement")]
    [SerializeField] private MovementController movementController;

    public MovementController Movement => movementController;

    protected override void Awake()
    {
        base.Awake();

        if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
        }
    }

    //public bool AddItem(Item item)
    //{
    //    if (item == null)
    //        return false;

    //    inventory.Add(item);
    //    return true;
    //}

    //public bool RemoveItem(Item item)
    //{
    //    if (item == null)
    //        return false;

    //    return inventory.Remove(item);
    //}
}
