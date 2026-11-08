using UnityEngine;

// MYTHRENOR'S HALLOW - DEFENSIVE VFX CONTROLLER
//
// PURPOSE:
// Handles visual effects for defensive combat actions.
// Supports guarding and directional dodging.
//
// TODO (FUTURE):
// Add impact VFX when an attack is successfully blocked.
//
// NOTE:
// CoreFunctions handles stamina, guarding, and dodging.
// This script only handles visual effects.

public class DefensiveVFXController : MonoBehaviour
{
    // Defensive References
    [SerializeField] private CoreFunctions coreFunctions;

    // Dodge VFX
    [SerializeField] private ParticleSystem dodgeVFX;
    [SerializeField] private TrailRenderer dodgeTrail;

    // Guard VFX
    [SerializeField] private ParticleSystem guardVFX;

    // PURPOSE:
    // Displays a protective shield while guarding.
    [SerializeField] private MeshRenderer guardForceField;

    // Tracks previous defensive states.
    private bool wasDodging;
    private bool wasBlocking;

    private void Awake()
    {
        // Find CoreFunctions if it wasn't assigned.
        if (coreFunctions == null)
        {
            coreFunctions = GetComponent<CoreFunctions>();
        }

        // Ensure defensive particles are initially stopped.
        if (dodgeVFX != null)
        {
            dodgeVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (dodgeTrail != null)
        {
            dodgeTrail.emitting = false;

            dodgeTrail.Clear();
        }

        if (guardVFX != null)
        {
            guardVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Show the protective shield.
        if (guardForceField != null)
        {
            guardForceField.enabled = false;
        }
    }

    private void Update()
    {
        if (coreFunctions == null)
        {
            return;
        }

        // Read the actual defensive states.
        bool isDodging = coreFunctions.IsDodging;

        bool isBlocking = coreFunctions.IsBlocking;

        // PURPOSE:
        // Update dodge particles only when dodge state changes.
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

        // PURPOSE:
        // Update guard particles only when guard state changes.
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
    // Starts the dodge particle trail.
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
    // Stops dodge emission when dodging ends.
    private void StopDodgeVFX()
    {
        if (dodgeVFX != null)
        {
            dodgeVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (dodgeTrail != null)
        {
            dodgeTrail.emitting = false;
        }
    }

    // PURPOSE:
    // Starts the protective guard aura.
    private void PlayGuardVFX()
    {
        if (guardVFX != null)
        {
            guardVFX.Play(true);
        }

        // Show the protective shield.
        if (guardForceField != null)
        {
            guardForceField.enabled = true;
        }
    }

    // PURPOSE:
    // Stops guard emission when guarding ends.
    private void StopGuardVFX()
    {
        if (guardVFX != null)
        {
            guardVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // Hide the shield when guarding ends.
        if (guardForceField != null)
        {
            guardForceField.enabled = false;
        }
    }

    // PURPOSE:
    // Clears defensive VFX when this component is disabled.
    private void OnDisable()
    {
        wasDodging = false;

        wasBlocking = false;

        if (dodgeVFX != null)
        {
            dodgeVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (dodgeTrail != null)
        {
            dodgeTrail.emitting = false;

            dodgeTrail.Clear();
        }

        if (guardVFX != null)
        {
            guardVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Ensure the shield disappears if this compenent is disabled.
        if (guardForceField != null)
        {
            guardForceField.enabled = false;
        }
    }
}
