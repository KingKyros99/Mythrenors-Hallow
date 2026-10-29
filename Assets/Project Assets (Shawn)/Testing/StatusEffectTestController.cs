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
    [SerializeField] private StatusEffectData burn;
    [SerializeField] private StatusEffectData bleed;
    [SerializeField] private StatusEffectData stun;
    [SerializeField] private StatusEffectData attackBuff;
    [SerializeField] private StatusEffectData defenseBuff;

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

    [ContextMenu("Test Burn")]
    public void TestBurn()
    {
        if (target != null && burn != null)
        {
            Debug.Log($"Burn applied: {target.ApplyEffect(burn)}");
        }
    }

    [ContextMenu("Test Bleed")]
    public void TestBleed()
    {
        if (target != null && bleed != null)
        {
            Debug.Log($"Bleed applied: {target.ApplyEffect(bleed)}");
        }
    }

    [ContextMenu("Test Stun")]
    public void TestStun()
    {
        if (target != null && stun != null)
        {
            Debug.Log($"Stun applied: {target.ApplyEffect(stun)}");
        }
    }

    [ContextMenu("Test Attack Buff")]
    public void TestAttackBuff()
    {
        if (target != null && attackBuff != null)
        {
            Debug.Log($"Attack Buff applied: {target.ApplyEffect(attackBuff)}");
        }
    }

    [ContextMenu("Test Defense Buff")]
    public void TestDefenseBuff()
    {
        if (target != null && defenseBuff != null)
        {
            Debug.Log($"Defense Buff applied: {target.ApplyEffect(defenseBuff)}");
        }
    }
}
