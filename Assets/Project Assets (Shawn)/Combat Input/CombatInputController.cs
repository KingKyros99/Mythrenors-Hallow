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

    // PURPOSE:
    // Plays the player's combat animations.
    // The Animator is attached to the character's visual model.
    // Combat Animation
    [SerializeField] private Animator playerAnimator;
    private static readonly int SwordAttackHash = Animator.StringToHash("SwordAttack");

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

    // Manual Aiming
    [SerializeField] private Camera aimCamera;

    // Determines what the reticle can aim at.
    // Exclude the Player layer in the Inspector.
    [SerializeField] private LayerMask aimLayers = ~0;

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
        RequestMeleeAttack();
    }

    // PURPOSE:
    // Starts the sword animation only when an attack
    // is accepted by MeleeManager.
    public void RequestMeleeAttack()
    {
        if (combatEntity == null || meleeManager == null || meleeAttack == null)
        {
            return;
        }

        // Preserve normal combat if no Animator is assigned.
        if (playerAnimator == null)
        {
            meleeManager.TryAttack(combatEntity, null, meleeAttack);

            return;
        }

        // For now, only begin sword swings from Locomotion.
        // Prevent starting another attack mid-animation.
        if (playerAnimator.IsInTransition(0) || !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
        {
            return;
        }

        bool attackStarted = meleeManager.TryBeginAnimatedAttack(combatEntity, null, meleeAttack);

        if (attackStarted)
        {
            playerAnimator.SetTrigger(SwordAttackHash);
        }
    }

    // PURPOSE:
    // Called by the sword animation's hit-frame event.
    public void ResolveSwordHitFrame()
    {
        if (combatEntity == null || meleeManager == null)
        {
            return;
        }

        meleeManager.ResolvePendingAnimatedAttack(combatEntity);
    }

    // PURPOSE:
    // Clears an unfinished sword attack when
    // the animation ends or is interrupted.
    public void CancelPendingSwordAttack()
    {
        if (combatEntity == null || meleeManager == null)
        {
            return;
        }

        meleeManager.CancelPendingAnimatedAttack(combatEntity);
    }

    // PURPOSE:
    // Fires a ranged attack using manual
    // camera-based reticle aiming.
    private void OnRanged(InputAction.CallbackContext context)
    {
        if (combatEntity == null || rangedManager == null || rangedAttack == null || rangedAttack.projectileData == null)
        {
            return;
        }

        // Get the actual projectile spawn position.
        Vector3 spawnPosition = GetFirePosition();

        // Calculate direction using the camera reticle.
        Vector3 direction = GetManualAimDirection(rangedAttack.projectileData.maxRange);

        // Fire without automatically selecting an enemy.
        rangedManager.TryFireManual(combatEntity, rangedAttack, spawnPosition, direction);
    }

    // PURPOSE:
    // Requests a spell using the Entity's mana.
    private void OnSpell(InputAction.CallbackContext context)
    {
        if (combatEntity == null || spellManager == null || spell == null || spell.projectileData == null)
        {
            return;
        }

        // Get the projectile's actual spawn position.
        Vector3 spawnPosition = GetFirePosition();

        // Calculate aim direction from the camera.
        Vector3 direction = GetManualAimDirection(spell.projectileData.maxRange);

        // Cast the spell without selecting an enemy.
        spellManager.TryCastManual(combatEntity, spell, spawnPosition, direction);
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

    // PURPOSE:
    // Calculates projectile direction using the
    // center of the player's camera.
    private Vector3 GetManualAimDirection(float maxRange)
    {
        if (aimCamera == null)
        {
            Debug.LogWarning("Combat aiming camera is missing.");

            return transform.forward;
        }

        // Cast a ray through the center of the screen.
        Ray aimRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Default to a point straight ahead.
        Vector3 aimPoint = aimRay.GetPoint(maxRange);

        // If the camera is pointing at something,
        // aim toward that object's collision point.
        if (Physics.Raycast(aimRay, out RaycastHit hit, maxRange, aimLayers, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        // Calculate direction from the actual FirePoint.
        Vector3 firePosition = GetFirePosition();

        Vector3 direction = aimPoint - firePosition;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return aimCamera.transform.forward;
        }

        return direction.normalized;
    }
}
