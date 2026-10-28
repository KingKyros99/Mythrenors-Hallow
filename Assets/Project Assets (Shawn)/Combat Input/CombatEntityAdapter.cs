
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
    public float Attack => characterBase != null ? characterBase.Attack : 0f;

    public float Defense => characterBase != null ? characterBase.Defense : 0f;

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

        // TODO (JABARI INTEGRATION):
        // Jabari's TakeDamage currently does not
        // automatically call Die() at zero HP.
        // Coordinate death handling with Jabari.
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

        return characterBase.SpendMana(Mathf.CeilToInt(amount));
    }
}
