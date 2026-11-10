using UnityEngine;

// MYTHRENOR'S HALLOW - DEFENSIVE VFX CONTROLLER
//
// PURPOSE:
// Controls visual effects for guarding, dodging,
// and successfully blocking incoming attacks.
//
// TODO (JABARI INTEGRATION):
// Listen for successful blocks from CoreFunctions.
// No damage or stamina calculations happen here.

public class DefensiveVFXController : MonoBehaviour
{
    // References
    [SerializeField] private CoreFunctions coreFunctions;

    // Dodge VFX
    [SerializeField] private ParticleSystem dodgeVFX;
    [SerializeField] private TrailRenderer dodgeTrail;

    // Guard VFX
    [SerializeField] private ParticleSystem guardVFX;
    [SerializeField] private MeshRenderer guardForceField;

    // Guard Impact VFX
    [SerializeField] private ParticleSystem guardImpactVFX;

    private bool wasDodging;
    private bool wasBlocking;

    private void Awake()
    {
        if (coreFunctions == null)
        {
            coreFunctions = GetComponent<CoreFunctions>();
        }

        // All effects begin disabled.
        StopDodgeVFX();

        StopGuardVFX();

        if (guardImpactVFX != null)
        {
            guardImpactVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnEnable()
    {
        if (coreFunctions == null)
        {
            coreFunctions = GetComponent<CoreFunctions>();
        }

        // Listen for an actual successful block.
        if (coreFunctions != null)
        {
            coreFunctions.OnSuccessfulBlock += PlayGuardImpactVFX;
        }
    }

    private void Update()
    {
        if (coreFunctions == null)
        {
            return;
        }

        // Detect dodge state changes.
        bool isDodging = coreFunctions.IsDodging;

        if (isDodging != wasDodging)
        {
            if (isDodging)
            {
                PlayDodgeVFX();
            }

            else
            {
                StopDodgeVFX();
            }

            wasDodging = isDodging;
        }

        // Detect guard state changes.
        bool isBlocking = coreFunctions.IsBlocking;

        if (isBlocking != wasBlocking)
        {
            if (isBlocking)
            {
                PlayGuardVFX();
            }

            else
            {
                StopGuardVFX();
            }

            wasBlocking = isBlocking;
        }
    }

    // PURPOSE:
    // Starts dodge particles and the movement trail.
    private void PlayDodgeVFX()
    {
        if (dodgeVFX != null)
        {
            dodgeVFX.Play(true);
        }

        if (dodgeTrail != null)
        {
            dodgeTrail.Clear();

            dodgeTrail.emitting = true;
        }
    }

    // PURPOSE:
    // Stops dodge particles and removes the trail.
    private void StopDodgeVFX()
    {
        if (dodgeVFX != null)
        {
            dodgeVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (dodgeTrail != null)
        {
            dodgeTrail.emitting = false;

            dodgeTrail.Clear();
        }
    }

    // PURPOSE:
    // Shows the force field while guarding.
    private void PlayGuardVFX()
    {
        if (guardVFX != null)
        {
            guardVFX.Play(true);
        }

        if (guardForceField != null)
        {
            guardForceField.enabled = true;
        }
    }

    // PURPOSE:
    // Hides the force field when guarding ends.
    private void StopGuardVFX()
    {
        if (guardVFX != null)
        {
            guardVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (guardForceField != null)
        {
            guardForceField.enabled = false;
        }
    }

    // PURPOSE:
    // Plays one particle burst after a successful block.
    private void PlayGuardImpactVFX()
    {
        if (guardImpactVFX == null)
        {
            Debug.LogWarning("Guard Impact VFX is not assigned.");

            return;
        }

        guardImpactVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        guardImpactVFX.Play(true);

        Debug.Log("Guard Impact VFX triggered!");
    }

    // PURPOSE:
    // Allows testing without an attacking enemy.
    [ContextMenu("Preview Guard Impact")]
    private void PreviewGuardImpact()
    {
        PlayGuardImpactVFX();
    }

    private void OnDisable()
    {
        // Stop listening for block notifications.
        if (coreFunctions != null)
        {
            coreFunctions.OnSuccessfulBlock -= PlayGuardImpactVFX;
        }

        StopDodgeVFX();

        StopGuardVFX();

        if (guardImpactVFX != null)
        {
            guardImpactVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        wasDodging = false;

        wasBlocking = false;
    }
}
