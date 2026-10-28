using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEditor.Progress;

public class CoreFunctions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterBase characterBase;

    private void Awake()
    {
        if (characterBase == null)
        {
            characterBase = GetComponent<CharacterBase>();
        }
    }

    private void UpdateStaminaToggle()
    {
        if (characterBase == null)
            return;

        characterBase.ToggleStaminaRecovery(
            !IsBlocking && !IsDodging
        );
    }

    // ==================== DODGE STUFF ====================

    [Header("Dodge")]
    [SerializeField] private int dodgeCost = 25;
    [SerializeField] private float dodgeDuration = 1.1f;
    [SerializeField] private float invulnerableTime = 0.5f;

    public bool IsInvulnerable { get; private set; } = false;
    public bool IsDodging { get; private set; } = false;

    public bool Dodge()
    {
        if (IsDodging)
            return false;

        if (characterBase == null)
            return false;

        if (!characterBase.SpendStamina(dodgeCost))
            return false;

        StartCoroutine(SetDodging());

        return true;
    }

    // not gonna lie i had to search this shit up
    private IEnumerator SetDodging()
    {
        IsDodging = true;
        IsInvulnerable = true;
        UpdateStaminaToggle();

        yield return new WaitForSeconds(invulnerableTime);

        IsInvulnerable = false;

        //Seperates dodge animation time from Iframe time where we get there 
        yield return new WaitForSeconds(dodgeDuration - invulnerableTime);

        IsDodging = false;
        UpdateStaminaToggle();
    }

    // ==================== BLOCK ====================

    [Header("Block")]
    [SerializeField, Range(0f, 1f)]
    private float blockDamageReduction = 0.75f;

    [SerializeField] private int blockStaminaCost = 15;

    public bool IsBlocking { get; private set; } = false;

    //Enables and disables blocking
    public void SetBlocking(bool value)
    {
        // Cannot begin or maintain a block while dodging.
        if (IsDodging)
        {
            IsBlocking = false;
            UpdateStaminaToggle();
            return;
        }

        IsBlocking = value;
        UpdateStaminaToggle();
    }

    public bool Block(float incomingDamage, bool unblockable, out float remainingDamage)
    {
        // Default to taking the full incoming damage.
        remainingDamage = incomingDamage;

        // No block if we're not actively guarding.
        if (!IsBlocking)
            return false;

        // Unblockable attacks bypass the guard.
        if (unblockable)
            return false;

        // Cannot block without a valid stats component.
        if (characterBase == null)
            return false;

        // Stamina is spent only when a hit connects.
        if (!characterBase.SpendStamina(blockStaminaCost))
        {
            IsBlocking = false;
            return false;
        }

        // Apply the block's damage reduction.
        remainingDamage = incomingDamage *
            (1f - blockDamageReduction);

        return true;
    }
}
