using UnityEngine;
using UnityEngine.InputSystem;

namespace MythrenorHallow.UI
{
    // TEST ONLY - delete before release
    // fakes the numbers so i can try the HUD without the teammates code
    // 1 = -15 health, 2 = +15 health, 3 = -10 mana, 4 = refill mana
    // 7 = -20 stamina, 8 = refill stamina
    // 5 and 6 are for the enemy bar and continue button, not built yet
    public class UITestHarness : MonoBehaviour
    {
        [Header("What to test")]
        [SerializeField, Tooltip("The PlayerHUD in the scene.")]
        private PlayerHUD playerHUD;

        [Header("Fake numbers")]
        [SerializeField] private float maxHealth = 150f;
        [SerializeField] private float maxMana = 100f;
        [SerializeField] private float maxStamina = 100f;

        private float health;
        private float mana;
        private float stamina;

        private void Awake()
        {
            health = maxHealth;
            mana = maxMana;
            stamina = maxStamina;
        }

        private void Start()
        {
          
            if (playerHUD == null)
            {
                Debug.LogWarning("UITestHarness: Player HUD is not assigned.", this);
                return;
            }

            playerHUD.SetHealth(health, maxHealth);
            playerHUD.SetMana(mana, maxMana);
            playerHUD.SetStamina(stamina, maxStamina);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || playerHUD == null) return;

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                health = Mathf.Max(0f, health - 15f);
                playerHUD.SetHealth(health, maxHealth);
            }

            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                health = Mathf.Min(maxHealth, health + 15f);
                playerHUD.SetHealth(health, maxHealth);
            }

            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                mana = Mathf.Max(0f, mana - 10f);
                playerHUD.SetMana(mana, maxMana);
            }

            if (keyboard.digit4Key.wasPressedThisFrame)
            {
                mana = maxMana;
                playerHUD.SetMana(mana, maxMana);
            }

            if (keyboard.digit7Key.wasPressedThisFrame)
            {
                stamina = Mathf.Max(0f, stamina - 20f);
                playerHUD.SetStamina(stamina, maxStamina);
            }

            if (keyboard.digit8Key.wasPressedThisFrame)
            {
                stamina = maxStamina;
                playerHUD.SetStamina(stamina, maxStamina);
            }
        }
    }
}