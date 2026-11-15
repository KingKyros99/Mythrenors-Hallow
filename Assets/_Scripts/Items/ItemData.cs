using UnityEngine;

//Only Supports Consumables
public enum ItemCategory
{
    CONSUMABLE,
    EQUIPMENT
}

//Item Categories 
public enum ItemType
{
    NONE,
    HEALTH,
    MANA,
    BUFF,
    CLEANSE
}

//Future Gear Categories
public enum EquipmentType
{
    NONE,
    WEAPON,
    HEAD,
    CHEST,
    LEG
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Generator/Item")]

public class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public ItemCategory category;
    public ItemType itemType;
    public EquipmentType equipmentType;

    public Sprite icon;

    [Header("Effect Values")]
    public int effectValue;

    [Header("Stat Bonuses")]
    public int physique;
    public int might;
    public int nimble;
    public int focus;
    public int willpower;
    public int aptitude;

    [Header("Status Effect")]
    public StatusEffectData statusEffect;
}