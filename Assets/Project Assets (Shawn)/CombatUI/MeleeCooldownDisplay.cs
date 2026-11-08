using TMPro;
using UnityEngine;

// MYTHRENOR'S HALLOW - MELEE COOLDOWN DISPLAY
//
// PURPOSE:
// Displays the remaining melee attack cooldown
// on the Combat Hotbar.
//
// NOTE:
// Display only. Does not perform attacks or
// change the existing melee combat system.

public class MeleeCooldownDisplay : MonoBehaviour
{
    // Melee Attack References
    [SerializeField] private MeleeManager meleeManager;
    [SerializeField] private MeleeAttackData meleeAttack;

    // UI Reference
    [SerializeField] private TMP_Text cooldownText;

    // Display Colors
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.yellow;

    // PURPOSE:
    // Updates the displayed cooldown every frame.
    private void Update()
    {
        if (meleeManager == null || meleeAttack == null || cooldownText == null)
        {
            return;
        }

        float remainingCooldown = meleeManager.GetRemainingCooldown(meleeAttack);

        if (remainingCooldown > 0f)
        {
            cooldownText.text = $"{remainingCooldown:0.0}s";

            cooldownText.color = cooldownColor;
        }

        else
        {
            cooldownText.text = "CD Ready";

            cooldownText.color = readyColor;
        }
    }
}
