
using UnityEngine;

// MYTHRENOR'S HALLOW - COMBAT ENTITY ADAPTER
//
// PURPOSE:
// Connects Jabari's CharacterBase system to
// Shawn's reusable combat managers.
//
// TODO (JABARI INTEGRATION):
// Confirm team identification, death handling,
// and defensive interactions with CoreFunctions.
//
// NOTE:
// Character stats, health, and mana are provided
// by Jabari's CharacterBase, not stored here.

[RequireComponent(typeof(CharacterBase))]
public class CombatEntityAdapter : MonoBehaviour, ICombatEntity
{
    // Character References
    [SerializeField] private CharacterBase characterBase;
    [SerializeField] private StatusEffectManager statusEffectManager;

    // Team Settings
    [SerializeField] private int teamId;

    // PURPOSE:
    // Retrieves the CharacterBase attached
    // to this GameObject.
    private void Awake()
    {
        if (characterBase == null)
        {
            characterBase = GetComponent<CharacterBase>();
        }

        if (statusEffectManager == null)
        {
            statusEffectManager = GetComponent<StatusEffectManager>();
        }
    }

    // PURPOSE:
    // Provides the character's Transform
    // for targeting and range calculations.
    public Transform CombatTransform => transform;

    // PURPOSE:
    // Prevents combat actions when the
    // character is defeated.
    public bool IsAlive => characterBase != null && characterBase.CurrentHealth > 0 && !characterBase.IsDead;

    // TODO (JABARI INTEGRATION):
    // Replace Inspector-assigned team IDs if
    // Jabari implements a faction system.
    public int TeamId => teamId;

    // PURPOSE:
    // Reads the actual character statistics
    // from Jabari's CharacterBase.

    // PURPOSE:
    // Combines base Attack with temporary status bonuses.
    public float Attack
    {
        get
        {
            float baseAttack = characterBase != null ? characterBase.Attack : 0f;

            float bonus = statusEffectManager != null ? statusEffectManager.GetAttackBonus() : 0f;

            return baseAttack + bonus;
        }
    }

    // PURPOSE:
    // Combines base Defense with temporary status bonuses.
    public float Defense
    {
        get
        {
            float baseDefense = characterBase != null ? characterBase.Defense : 0f;

            float bonus = statusEffectManager != null ? statusEffectManager.GetDefenseBonus() : 0f;

            return baseDefense + bonus;
        }
    }

    public float CriticalChance => characterBase != null ? characterBase.CritChance : 0f;

    public float CriticalMultiplier => characterBase != null ? characterBase.CritMultiplier : 1f;

    // PURPOSE:
    // Forwards damage to Jabari's health system.
    public void ReceiveDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        int damage = Mathf.Max(1, Mathf.RoundToInt(amount));

        characterBase.TakeDamage(damage);

        // PURPOSE:
        // Temporarily displays actual health after damage.
        // TODO (DEBUG): Remove after integration testing.
        Debug.Log($"{name} Health: {characterBase.CurrentHealth}/{characterBase.MaxHealth}");
    }

    // PURPOSE:
    // Forwards healing to Jabari's health system.
    public void ReceiveHealing(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        characterBase.Heal(Mathf.CeilToInt(amount));
    }

    // PURPOSE:
    // Requests mana consumption through
    // Jabari's CharacterBase.
    public bool TrySpendMana(float amount)
    {
        if (!IsAlive || amount < 0f)
        {
            return false;
        }

        // CharacterBase uses integer mana values.
        // Round upward to avoid undercharging spells.
        if (amount == 0f)
        {
            return true;
        }

        int manaCost = Mathf.CeilToInt(amount);

        bool successful = characterBase.SpendMana(manaCost);

        Debug.Log($"{name} Mana: {characterBase.CurrentMana}/{characterBase.MaxMana} | Cost: {manaCost} | Success: {successful}");

        return successful;
    }

    // PURPOSE:
    // Restores mana through Jabari's CharacterBase
    // when a spell fails after spending resources.
    public void RefundMana(float amount)
    {
        if (characterBase == null || amount <= 0f)
        {
            return;
        }

        int manaAmount = Mathf.CeilToInt(amount);

        characterBase.RestoreMana(manaAmount);
    }

}
