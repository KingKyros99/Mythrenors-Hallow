using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public bool UseItem(ItemData item, CombatEntityAdapter target)
    {
        if (item == null || target == null || !target.IsAlive)
            return false;

        // Consumables only for now.
        if (item.category != ItemCategory.CONSUMABLE)
            return false;

        return UseConsumable(item, target);
    }

    public bool UseConsumable(ItemData item, CombatEntityAdapter target)
    {
        if (item == null || target == null || !target.IsAlive)
            return false;

        CharacterBase character = target.GetComponent<CharacterBase>();

        if (character == null)
        {
            //Debug.LogWarning("No CharacterBase found on " + target.name);
            return false;
        }

        switch (item.itemType)
        {
            case ItemType.HEALTH:
                if (character.CurrentHealth >= character.MaxHealth)
                    return false;

                character.Heal(item.effectValue);
                return true;

            case ItemType.MANA:
                if (character.CurrentMana >= character.MaxMana)
                    return false;

                character.RestoreMana(item.effectValue);
                return true;

            case ItemType.BUFF:
                {
                    StatusEffectManager statusManager =
                        target.GetComponent<StatusEffectManager>();

                    if (statusManager == null)
                    {
                        //Debug.LogWarning("No StatusEffectManager found on " + target.name);
                        return false;
                    }

                    if (item.statusEffect == null)
                    {
                        //Debug.LogWarning("No status effect assigned to " + item.itemName);
                        return false;
                    }

                    return statusManager.ApplyEffect(item.statusEffect);
                }

            case ItemType.CLEANSE:
                {
                    StatusEffectManager statusManager =
                        target.GetComponent<StatusEffectManager>();

                    if (statusManager == null)
                    {
                        //Debug.LogWarning("No StatusEffectManager found on " + target.name);
                        return false;
                    }

                    statusManager.ClearEffects();

                    //Debug.Log(item.itemName + " cleansed all effects from " + target.name);
                    return true;
                }

            default:
                //Debug.LogWarning("Unsupported consumable type: " + item.itemType);
                return false;
        }
    }
}
