using System.Runtime.InteropServices;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string charName;

    private CoreFunctions coreFunctions;

    private void Awake()
    {
        health = maxHealth;
        mana = maxMana;
        stamina = maxStamina;

        coreFunctions = GetComponent<CoreFunctions>();
    }

    // ==================== HEALTH STUFF ====================

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int health;

    public int CurrentHealth => health;
    public int MaxHealth => maxHealth;

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        health = Mathf.Max(0, health - amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        health = Mathf.Min(health + amount, maxHealth);
    }

    // ==================== MANA STUFF ====================

    [Header("Mana")]
    [SerializeField] private int maxMana = 70;
    private int mana;

    public int CurrentMana => mana;
    public int MaxMana => maxMana;

    public bool SpendMana(int amount)
    {
        if (amount <= 0 || mana < amount)
            return false;

        mana -= amount;
        return true;
    }

    public void RestoreMana(int amount)
    {
        if (amount <= 0)
            return;

        mana = Mathf.Min(mana + amount, maxMana);
    }

    // ==================== STAMINA STUFF ====================

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 50f;
    private float stamina;

    [Header("Stamina Recovery")]
    [SerializeField] private float staminaRecoveryRate = 15f;
    [SerializeField] private float staminaRecoveryDelay = 1f;
    private float staminaRecoveryTimer;
    private bool canRecoverStamina = true;

    public float CurrentStamina => stamina;
    public float MaxStamina => maxStamina;

    public bool SpendStamina(int amount)
    {
        if (amount <= 0 || stamina < amount)
            return false;

        stamina -= amount;
        staminaRecoveryTimer = staminaRecoveryDelay;
        return true;
    }

    public void RestoreStamina(float amount)
    {
        if (amount <= 0) 
            return;

        stamina = Mathf.Min(stamina + amount, maxStamina);
    }

    //Turns Recovery Off/On
    public void ToggleStaminaRecovery(bool allowed)
    {
        canRecoverStamina = allowed;
    }

    // Certain Attributes/Buffs may affect this later
    public void StaminaRecoveryRate()
    {
        //Prevents Recovery if prohibited
        if(!canRecoverStamina) 
            return;

        // Wait until the recovery delay expires.
        if (staminaRecoveryTimer > 0f)
        {
            staminaRecoveryTimer -= Time.deltaTime;
            staminaRecoveryTimer= Mathf.Max(staminaRecoveryTimer, 0f);
            return;
        }

        // Recover stamina naturally over time.
        RestoreStamina(staminaRecoveryRate * Time.deltaTime);
    }

    // ==================== DEATH THING ====================

    private bool mIsDead = false;

    public bool IsDead => mIsDead;

    public virtual void Die()
    {
        if (mIsDead)
            return;

        mIsDead = true;

        Debug.Log($"{name} has died.");
    }

    // ==================== STAT STUFF ====================

    [Header("Combat Stats")]
    [SerializeField] private int attack = 2;
    [SerializeField] private int defense = 2;
    [SerializeField] private int critChance = 2;
    [SerializeField] private float critMult = 1.1f;
    [SerializeField] private int dodgeChance = 2; /*thinking of this being just a passive chance for attacks to 'graze'*/

    [Header("Resistances")]
    [SerializeField] private int mStatusResistance = 0; /*Optional shit for this week if Shawn gets to it*/
    [SerializeField] private int mCritResistance = 0;

    private void Update()
    {
       StaminaRecoveryRate();
    }
}
