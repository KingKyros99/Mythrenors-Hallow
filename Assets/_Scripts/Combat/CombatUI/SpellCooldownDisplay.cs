using TMPro;
using UnityEngine;

// MYTHRENOR'S HALLOW - SPELL COOLDOWN DISPLAY
//
// PURPOSE:
// Displays a spell's mana cost and remaining
// cooldown on the Combat Hotbar.
//
// NOTE:
// Display only. Does not cast spells or spend mana.

public class SpellCooldownDisplay : MonoBehaviour
{
    // Spell References
    [SerializeField] private SpellManager spellManager;
    [SerializeField] private SpellData spellData;
    [SerializeField] private CharacterBase playerCharacter;

    // UI Reference
    [SerializeField] private TMP_Text cooldownText;

    // Display Colors
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.yellow;
    [SerializeField] private Color insufficientManaColor = Color.red;

    // PURPOSE:
    // Updates the spell's cooldown display.
    private void Update()
    {
        if (spellManager == null || spellData == null || playerCharacter == null || cooldownText == null)
        {
            return;
        }

        float remainingCooldown = spellManager.GetRemainingCooldown(spellData);

        // Match the mana rounding used by
        // CombatEntityAdapter.TrySpendMana().
        int manaCost = Mathf.CeilToInt(Mathf.Max(0f, spellData.manaCost));

        int currentMana = playerCharacter.CurrentMana;

        // Check whether the player can afford the spell.
        bool hasEnoughMana = currentMana >= manaCost;

        if (remainingCooldown > 0f)
        {
            cooldownText.text = $"MP: {manaCost} | {remainingCooldown:0.0}s";

            cooldownText.color = cooldownColor;
        }

        else if (!hasEnoughMana)
        {
            cooldownText.text = $"MP: {manaCost} | LOW MANA";

            cooldownText.color = insufficientManaColor;
        }

        else
        {
            cooldownText.text = $"MP: {manaCost} | CD Ready";

            cooldownText.color = readyColor;
        }
    }
}
