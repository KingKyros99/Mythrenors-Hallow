using UnityEngine;

// MYTHRENOR'S HALLOW - PLAYER HIT REACTION
//
// PURPOSE:
// Plays the player's hit reaction animation when
// damage is received.
//
// TODO (JABARI INTEGRATION):
// Eventually distinguish direct enemy hits from
// status-effect damage using the combat system.
//
// This script does not calculate or apply damage.

public class PlayerHitReaction : MonoBehaviour
{
    // References
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private CharacterBase characterBase;
    [SerializeField] private CoreFunctions coreFunctions;

    // Must match the Animator Trigger name.
    private static readonly int TakeHitTrigger = Animator.StringToHash("TakeHit");

    private void Awake()
    {
        FindReferences();
    }

    // PURPOSE:
    // Finds any references not assigned in the Inspector.
    private void FindReferences()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        if (characterBase == null)
        {
            characterBase = GetComponent<CharacterBase>();
        }

        if (coreFunctions == null)
        {
            coreFunctions = GetComponent<CoreFunctions>();
        }
    }

    // PURPOSE:
    // Subscribe to the character's damage event.
    private void OnEnable()
    {
        FindReferences();

        if (characterBase != null)
        {
            characterBase.OnDamageTaken += HandleDamageTaken;

            Debug.Log("PlayerHitReaction subscribed to OnDamageTaken!");
        }

        else
        {
            Debug.LogWarning("PlayerHitReaction: CharacterBase is missing.");
        }
    }

    // PURPOSE:
    // Unsubscribe when this component is disabled.
    private void OnDisable()
    {
        if (characterBase != null)
        {
            characterBase.OnDamageTaken -= HandleDamageTaken;
        }
    }

    // PURPOSE:
    // Checks whether the player should react to damage.
    private void HandleDamageTaken(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        // Don't play a normal hit reaction after death.
        if (characterBase != null && characterBase.IsDead)
        {
            return;
        }

        // Don't interrupt guarding or dodging.
        if (coreFunctions != null)
        {
            if (coreFunctions.IsBlocking || coreFunctions.IsDodging || coreFunctions.IsInvulnerable)
            {
                return;
            }
        }

        PlayHitReaction();
    }

    // PURPOSE:
    // Triggers the player's hit reaction animation.
    public void PlayHitReaction()
    {
        if (playerAnimator == null)
        {
            Debug.LogWarning("PlayerHitReaction: Animator is missing.");

            return;
        }

        playerAnimator.SetTrigger(TakeHitTrigger);

        Debug.Log("Player Hit Reaction triggered!");
    }

    // PURPOSE:
    // Manually tests the animation in Unity.
    [ContextMenu("Test Hit Reaction")]
    private void TestHitReaction()
    {
        PlayHitReaction();
    }
}
