using UnityEngine;
using UnityEngine.InputSystem;

// MYTHRENOR'S HALLOW - COMBAT INPUT CONTROLLER
//
// PURPOSE:
// Handles combat input and sends attack requests
// to the appropriate combat managers.
//
// TODO (JABARI INTEGRATION):
// Replace the temporary combat Entity references
// with Jabari's Entity or adapter components.
//
// TODO (HUNTER INTEGRATION):
// Coordinate Input Actions so combat input
// works alongside the movement controller.
//
// NOTE:
// Input bindings and attack settings are
// configured through Unity's Inspector.

public class CombatInputController : MonoBehaviour
{
    // Combat Entity
    [SerializeField] private MonoBehaviour entityComponent;

    // Combat Managers
    [SerializeField] private MeleeManager meleeManager;
    [SerializeField] private RangedAttackManager rangedManager;
    [SerializeField] private SpellManager spellManager;

    // Attack Data
    [SerializeField] private MeleeAttackData meleeAttack;
    [SerializeField] private RangedAttackData rangedAttack;
    [SerializeField] private SpellData spell;

    // Input Actions
    [SerializeField] private InputActionReference meleeInput;
    [SerializeField] private InputActionReference rangedInput;
    [SerializeField] private InputActionReference spellInput;

    // Projectile Spawn
    [SerializeField] private Transform firePoint;

    // Self Status Abilities
    [SerializeField] private SelfStatusAbilityManager selfAbilityManager;

    [SerializeField] private SelfStatusAbilityData regenerationAbility;
    [SerializeField] private SelfStatusAbilityData attackBoostAbility;
    [SerializeField] private SelfStatusAbilityData defenseBoostAbility;

    // Ability Input Actions
    [SerializeField] private InputActionReference regenerationInput;
    [SerializeField] private InputActionReference attackBoostInput;
    [SerializeField] private InputActionReference defenseBoostInput;


    // PURPOSE:
    // Store PlayerInput's runtime actions for abilities.
    private PlayerInput playerInput;
    private InputAction regenerationAction;
    private InputAction attackBoostAction;
    private InputAction defenseBoostAction;


    private ICombatEntity combatEntity;

    // PURPOSE:
    // Retrieves compatible combat Entities from
    // the components assigned in the Inspector.
    private void Awake()
    {
        combatEntity = entityComponent as ICombatEntity;

        if (combatEntity == null)
        {
            Debug.LogWarning($"{name} has no valid combat Entity assigned.");
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: FirePoint is missing. Ranged attacks and spells will be unavailable.", this);
        }
    }

    // PURPOSE:
    // Registers combat input callbacks.
    // Input Actions are enabled by PlayerInput.
    private void OnEnable()
    {
        if (meleeInput != null)
        {
            meleeInput.action.performed += OnMelee;
        }

        if (rangedInput != null)
        {
            rangedInput.action.performed += OnRanged;
        }

        if (spellInput != null)
        {
            spellInput.action.performed += OnSpell;
        }

        // Find the PlayerInput component on our character.
        playerInput = GetComponent<PlayerInput>();

        // Find the runtime copies of our ability actions.
        regenerationAction = GetPlayerAction(regenerationInput);
        attackBoostAction = GetPlayerAction(attackBoostInput);
        defenseBoostAction = GetPlayerAction(defenseBoostInput);

        // Subscribe to the correct actions.
        if (regenerationAction != null)
        {
            regenerationAction.performed += OnRegenerationRequested;
        }

        if (attackBoostAction != null)
        {
            attackBoostAction.performed += OnAttackBoostRequested;
        }

        if (defenseBoostAction != null)
        {
            defenseBoostAction.performed += OnDefenseBoostRequested;
        }

    }

    // PURPOSE:
    // Unregisters callbacks to prevent duplicate
    // attack requests when this script is disabled.
    private void OnDisable()
    {
        if (meleeInput != null)
        {
            meleeInput.action.performed -= OnMelee;
        }

        if (rangedInput != null)
        {
            rangedInput.action.performed -= OnRanged;
        }

        if (spellInput != null)
        {
            spellInput.action.performed -= OnSpell;
        }

        // Unsubscribe from the exact runtime actions
        // that were registered in OnEnable.
        if (regenerationAction != null)
        {
            regenerationAction.performed -= OnRegenerationRequested;
        }

        if (attackBoostAction != null)
        {
            attackBoostAction.performed -= OnAttackBoostRequested;
        }

        if (defenseBoostAction != null)
        {
            defenseBoostAction.performed -= OnDefenseBoostRequested;
        }

        // NOTE:
        // Input actions may be shared with Hunter's
        // controller, so we do not disable them here.
    }

    // PURPOSE:
    // Requests a melee attack when its input fires.
    private void OnMelee(InputAction.CallbackContext context)
    {
        if (combatEntity == null || meleeManager == null || meleeAttack == null)
        {
            return;
        }

        // MeleeManager handles its own target selection
        meleeManager.TryAttack(combatEntity, null, meleeAttack);
    }

    // PURPOSE:
    // Requests a ranged projectile attack.
    private void OnRanged(InputAction.CallbackContext context)
    {
        if (combatEntity == null || rangedManager == null || rangedAttack == null)
        {
            return;
        }

        if (!combatEntity.IsAlive || firePoint == null)
        {
            return;
        }

        if (rangedAttack.targetingData == null)
        {
            return;
        }

        ICombatEntity target = TargetingSystem.FindNearestEnemy(combatEntity, rangedAttack.targetingData);

        if (target == null)
        {
            Debug.Log("No valid ranged target found.");

            return;
        }

        rangedManager.TryFire(combatEntity, target, rangedAttack, firePoint.position);
    }

    // PURPOSE:
    // Requests a spell using the Entity's mana.
    private void OnSpell(InputAction.CallbackContext context)
    {
        if (combatEntity == null || spellManager == null || spell == null)
        {
            return;
        }

        if (!combatEntity.IsAlive || firePoint == null)
        {
            return;
        }

        if (spell.targetingData == null)
        {
            return;
        }

        ICombatEntity target = TargetingSystem.FindNearestEnemy(combatEntity, spell.targetingData);

        if (target == null)
        {
            Debug.Log("No valid spell target found.");

            return;
        }

        spellManager.TryCast(combatEntity, target, spell, firePoint.position);
    }

    // PURPOSE:
    // Finds the PlayerInput version of an assigned action.
    private InputAction GetPlayerAction(InputActionReference reference)
    {
        if (playerInput == null || playerInput.actions == null || reference == null || reference.action == null)
        {
            return null;
        }

        return playerInput.actions.FindAction(reference.action.id);
    }

    // PURPOSE:
    // Requests the Player's Regeneration ability.
    private void OnRegenerationRequested(InputAction.CallbackContext context)
    {
        if (selfAbilityManager != null && regenerationAbility != null)
        {
            selfAbilityManager.TryActivate(regenerationAbility);
        }
    }

    // PURPOSE:
    // Requests the Player's Attack Boost ability.
    private void OnAttackBoostRequested(InputAction.CallbackContext context)
    {
        if (selfAbilityManager != null && attackBoostAbility != null)
        {
            selfAbilityManager.TryActivate(attackBoostAbility);
        }
    }

    // PURPOSE:
    // Requests the Player's Defense Boost ability.
    private void OnDefenseBoostRequested(InputAction.CallbackContext context)
    {
        if (selfAbilityManager != null && defenseBoostAbility != null)
        {
            selfAbilityManager.TryActivate(defenseBoostAbility);
        }
    }

    // PURPOSE:
    // Returns the configured projectile spawn position.
    private Vector3 GetFirePosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        if (combatEntity != null && combatEntity.CombatTransform != null)
        {
            return combatEntity.CombatTransform.position;
        }

        return transform.position;
    }
}
