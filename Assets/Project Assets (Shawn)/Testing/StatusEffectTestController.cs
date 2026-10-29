using UnityEngine;

// MYTHRENOR'S HALLOW - STATUS EFFECT TEST CONTROLLER
//
// PURPOSE:
// Manually tests status effect application
// through Unity's Inspector.
//
// NOTE:
// TESTING ONLY. Not used in gameplay scenes.

public class StatusEffectTestController : MonoBehaviour
{
    [SerializeField] private StatusEffectManager target;
    [SerializeField] private StatusEffectData poison;
    [SerializeField] private StatusEffectData regeneration;

    [ContextMenu("Test Poison")]
    public void TestPoison()
    {
        if (target != null && poison != null)
        {
            Debug.Log($"Poison applied: {target.ApplyEffect(poison)}");
        }
    }

    [ContextMenu("Test Regeneration")]
    public void TestRegeneration()
    {
        if (target != null && regeneration != null)
        {
            Debug.Log($"Regeneration applied: {target.ApplyEffect(regeneration)}");
        }
    }

    [ContextMenu("Clear Status Effects")]
    public void ClearStatusEffects()
    {
        if (target != null)
        {
            target.ClearEffects();
        }
    }
}
