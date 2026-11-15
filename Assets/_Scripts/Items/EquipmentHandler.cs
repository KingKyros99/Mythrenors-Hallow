
using UnityEngine;

//public class EquipmentHandler : MonoBehaviour
//{
//    [Header("Equipment Slots")]
//    [SerializeField] private ItemData weapon;
//    [SerializeField] private ItemData head;
//    [SerializeField] private ItemData chest;
//    [SerializeField] private ItemData leg;

//    private Entity entity;

//    private void Awake()
//    {
//        entity = GetComponent<Entity>();

//        if (entity == null)
//        {
//            Debug.LogError("EquipmentHandler needs a Entity.");
//        }
//    }

//    public bool Equip(ItemData item)
//    {
//        if (item == null || item.category != ItemCategory.EQUIPMENT || item.equipmentType == EquipmentType.NONE)
//        {
//            return false;
//        }

//        switch (item.equipmentType)
//        {
//            case EquipmentType.WEAPON:
//                Unequip(EquipmentType.WEAPON);
//                weapon = item;
//                break;

//            case EquipmentType.HEAD:
//                Unequip(EquipmentType.HEAD);
//                head = item;
//                break;

//            case EquipmentType.CHEST:
//                Unequip(EquipmentType.CHEST);
//                chest = item;
//                break;

//            case EquipmentType.LEG:
//                Unequip(EquipmentType.LEG);
//                leg = item;
//                break;

//            default:
//                return false;
//        }

//        ApplyBonuses(item, 1);
//        return true;
//    }

//    public void Unequip(EquipmentType slot)
//    {
//        ItemData item = null;

//        switch (slot)
//        {
//            case EquipmentType.WEAPON:
//                item = weapon;
//                weapon = null;
//                break;

//            case EquipmentType.HEAD:
//                item = head;
//                head = null;
//                break;

//            case EquipmentType.CHEST:
//                item = chest;
//                chest = null;
//                break;

//            case EquipmentType.LEG:
//                item = leg;
//                leg = null;
//                break;
//        }

//        if (item != null)
//            ApplyBonuses(item, -1);
//    }

//    private void ApplyBonuses(ItemData item, int multiplier)
//    {
//        if (entity == null)
//            return;

//        entity.AddStatModifier(
//            StatType.Physique, item.physique * multiplier);

//        entity.AddStatModifier(
//            StatType.Might, item.might * multiplier);

//        entity.AddStatModifier(
//            StatType.Nimble, item.nimble * multiplier);

//        entity.AddStatModifier(
//            StatType.Focus, item.focus * multiplier);

//        entity.AddStatModifier(
//            StatType.Willpower, item.willpower * multiplier);

//        entity.AddStatModifier(
//            StatType.Aptitude, item.aptitude * multiplier);
//    }
//}