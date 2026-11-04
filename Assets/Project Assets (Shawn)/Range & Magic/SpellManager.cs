using System.Collections.Generic;
using UnityEngine;

// MYTHRENOR'S HALLOW - SPELL MANAGER
//
// PURPOSE:
// Handles spellcasting requests, mana costs,
// target validation, and spell cooldowns.
//
// TODO (JABARI INTEGRATION):
// Connect spellcasting to Jabari's Player
// and Enemy controllers.
//
// NOTE:
// This version supports projectile damage
// spells using the shared ProjectileManager.

public class SpellManager : MonoBehaviour
{
    // Manager References
    [SerializeField] private ProjectileManager projectileManager;

    // PURPOSE:
    // Tracks spell cooldowns per manager instance.
    // Shared SpellData assets do not store cooldowns.
    private Dictionary<SpellData, float> nextCastTimes = new Dictionary<SpellData, float>();

    // PURPOSE:
    // Checks whether the caster and spell are valid
    // and whether the spell cooldown has finished.
    //
    // NOTE:
    // Mana availability is checked by TrySpendMana
    // during the actual casting request.
    public bool CanCast(ICombatEntity caster, SpellData spellData)
    {
        if (caster == null || spellData == null)
        {
            return false;
        }

        if (!StatusEffectManager.CanEntityAct(caster))
        {
            return false;
        }

        if (projectileManager == null || spellData.projectileData == null || spellData.targetingData == null)
        {
            return false;
        }

        ProjectileData projectile = spellData.projectileData;

        if (projectile.projectilePrefab == null || projectile.speed <= 0f || projectile.maxRange <= 0f || projectile.lifetime <= 0f)
        {
            return false;
        }

        if (nextCastTimes.TryGetValue(spellData, out float nextCastTime))
        {
            if (Time.time < nextCastTime)
            {
                return false;
            }
        }

        return true;
    }

    // PURPOSE:
    // Attempts to cast a projectile spell at a target.
    //
    // TODO (JABARI INTEGRATION):
    // Verify mana spending through Jabari's
    // CharacterStats or CoreFunctions system.
    public bool TryCast(ICombatEntity caster, ICombatEntity target, SpellData spellData, Vector3 spawnPosition)
    {
        if (!CanCast(caster, spellData))
        {
            return false;
        }

        // Validate the target before spending mana.
        if (!TargetingSystem.IsValidTarget(caster, target, spellData.targetingData))
        {
            return false;
        }

        // Aim toward the target's collider center.
        Vector3 aimPoint = ProjectileAimUtility.GetAimPoint(target);

        Vector3 direction = aimPoint - spawnPosition;

        if (direction.sqrMagnitude <= 0f)
        {
            return false;
        }

        // Ask the Entity to validate and spend mana.
        if (!caster.TrySpendMana(spellData.manaCost))
        {
            return false;
        }

        // Use the same projectile system as
        // bows, crossbows, and thrown weapons.
        bool successful = projectileManager.TrySpawnProjectile(caster, spellData.projectileData, spellData.damage, spawnPosition,
            direction, spellData.statusEffect);

        if (!successful)
        {
            // PURPOSE:
            // Return mana if projectile creation fails
            // CharacterBase remains responsible for mana.
            caster.RefundMana(spellData.manaCost);

            Debug.LogWarning($"{caster.CombatTransform.name} failed to cast {spellData.spellName}. Mana refunded.");

            return false;
        }

        // Begin the cooldown after spawning.
        nextCastTimes[spellData] = Time.time + Mathf.Max(0f, spellData.cooldown);

        return true;
    }

    // PURPOSE:
    // Returns the remaining cooldown for a spell.
    public float GetRemainingCooldown(SpellData spellData)
    {
        if (spellData == null)
        {
            return 0f;
        }

        if (!nextCastTimes.TryGetValue(spellData, out float nextCastTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, nextCastTime - Time.time);
    }

    // PURPOSE:
    // Resets the cooldown of one spell.
    public void ResetCooldown(SpellData spellData)
    {
        if (spellData == null)
        {
            return;
        }

        nextCastTimes.Remove(spellData);
    }
}
