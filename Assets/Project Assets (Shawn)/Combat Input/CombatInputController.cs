
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
    [SerializeField] private MonoBehaviour targetComponent;

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

    private ICombatEntity combatEntity;
    private ICombatEntity targetEntity;

    // PURPOSE:
    // Retrieves compatible combat Entities from
    // the components assigned in the Inspector.
    private void Awake()
    {
        combatEntity = entityComponent as ICombatEntity;
        targetEntity = targetComponent as ICombatEntity;
    }

    // PURPOSE:
    // Registers combat input callbacks.
    private void OnEnable()
    {
        if (meleeInput != null)
        {
            meleeInput.action.performed += OnMelee;
            meleeInput.action.Enable();
        }

        if (rangedInput != null)
        {
            rangedInput.action.performed += OnRanged;
            rangedInput.action.Enable();
        }

        if (spellInput != null)
        {
            spellInput.action.performed += OnSpell;
            spellInput.action.Enable();
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

        // NOTE:
        // Input actions may be shared with Hunter's
        // controller, so we do not disable them here.
    }

    // PURPOSE:
    // Requests a melee attack when its input fires.
    private void OnMelee(InputAction.CallbackContext context)
    {
        if (combatEntity == null || targetEntity == null || meleeManager == null || meleeAttack == null)
        {
            return;
        }

        meleeManager.TryAttack(combatEntity, targetEntity, meleeAttack);
    }

    // PURPOSE:
    // Requests a ranged projectile attack.
    private void OnRanged(InputAction.CallbackContext context)
    {
        if (combatEntity == null || targetEntity == null || rangedManager == null || rangedAttack == null)
        {
            return;
        }

        rangedManager.TryFire(combatEntity, targetEntity, rangedAttack, GetFirePosition());
    }

    // PURPOSE:
    // Requests a spell using the Entity's mana.
    private void OnSpell(InputAction.CallbackContext context)
    {
        if (combatEntity == null || targetEntity == null || spellManager == null || spell == null)
        {
            return;
        }

        spellManager.TryCast(combatEntity, targetEntity, spell, GetFirePosition());
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
