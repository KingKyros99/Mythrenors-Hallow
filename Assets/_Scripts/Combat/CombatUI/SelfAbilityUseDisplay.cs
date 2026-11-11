using TMPro;
using UnityEngine;

// MYTHRENOR'S HALLOW - SELF ABILITY USE DISPLAY
//
// PURPOSE:
// Displays the remaining and maximum uses of
// a configured self-ability on the Combat Hotbar.
//
// NOTE:
// Display only. Does not activate abilities or
// modify the character's combat resources.

public class SelfAbilityUseDisplay : MonoBehaviour
{
    // Ability References
    [SerializeField] private SelfStatusAbilityManager abilityManager;
    [SerializeField] private SelfStatusAbilityData abilityData;

    // UI References
    [SerializeField] private TMP_Text usesText;
    [SerializeField] private TMP_Text cooldownText;

    // Charge Display Colors
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color exhaustedColor = Color.red;
    [SerializeField] private Color cooldownColor = Color.yellow;

    private int previousRemaining = -1;
    private int previousMaximum = -1;

    // PURPOSE:
    // Checks whether the displayed values need updating.
    private void Update()
    {
        if (abilityManager == null || abilityData == null || usesText == null)
        {
            return;
        }

        int remaining = abilityManager.GetRemainingUses(abilityData);

        int maximum = abilityManager.GetMaxUses(abilityData);

        // Update the remaining-use display only
        // when its values change.
        if (remaining != previousRemaining || maximum != previousMaximum)
        {
            usesText.text = $"{remaining}/{maximum}";

            usesText.color = remaining <= 0 ? exhaustedColor : availableColor;

            previousRemaining = remaining;

            previousMaximum = maximum;
        }

        // Update the cooldown independently so
        // its countdown continues every frame.
        if (cooldownText != null)
        {
            float cooldown = abilityManager.GetRemainingCooldown(abilityData);

            if (remaining <= 0)
            {
                cooldownText.text = "EXHAUSTED";

                cooldownText.color = exhaustedColor;
            }

            else if (cooldown > 0f)
            {
                cooldownText.text = $"{cooldown:0.0}s";

                cooldownText.color = cooldownColor;
            }

            else
            {
                cooldownText.text = "CD Ready";

                cooldownText.color = availableColor;
            }
        }
    }
}
