using TMPro;
using UnityEngine;

// MYTHRENOR'S HALLOW - RANGED COOLDOWN DISPLAY
//
// PURPOSE:
// Displays ranged attack cooldown information
// on the Combat Hotbar.
//
// NOTE:
// Display only. Does not fire arrows or consume ammo.
//
// TODO (JABARI INTEGRATION):
// Replace the unlimited ammo indicator when
// inventory and ammunition are implemented.

public class RangedCooldownDisplay : MonoBehaviour
{
    // Ranged Attack References
    [SerializeField] private RangedAttackManager rangedManager;
    [SerializeField] private RangedAttackData rangedAttack;

    // UI Reference
    [SerializeField] private TMP_Text cooldownText;

    // Display Colors
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.yellow;

    // PURPOSE:
    // Updates the ranged attack cooldown display.
    private void Update()
    {
        if (rangedManager == null || rangedAttack == null || cooldownText == null)
        {
            return;
        }

        float remainingCooldown = rangedManager.GetRemainingCooldown(rangedAttack);

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
