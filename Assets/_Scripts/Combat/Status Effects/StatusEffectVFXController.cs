using System.Collections.Generic;
using UnityEngine;

// MYTHRENOR'S HALLOW - STATUS EFFECT VFX CONTROLLER
//
// PURPOSE:
// Handles visual effects for active status effects.
// Supports both player and enemy combat Entities.
//
// NOTE:
// StatusEffectManager controls when effects begin and end.
// This script only handles their visual representation.

public class StatusEffectVFXController : MonoBehaviour
{
    // VFX References

    // Status Effect VFX
    [SerializeField] private Transform effectAnchor;

    // PURPOSE:
    // Tracks spawned VFX so multiple copies of the
    // same status effect cannot exist simultaneously.
    private readonly Dictionary<StatusEffectData, GameObject> activeVFX = new Dictionary<StatusEffectData, GameObject>();

    private void Awake()
    {
        // Default to this character if no anchor is assigned.
        if (effectAnchor == null)
        {
            effectAnchor = transform;
        }
    }

    // PURPOSE:
    // Spawns the VFX associated with a status effect.
    // Existing VFX will not be spawned again.
    public void PlayEffect(StatusEffectData data)
    {
        if (data == null || data.vfxPrefab == null)
        {
            return;
        }

        // Prevent duplicate visual effects.
        if (activeVFX.ContainsKey(data))
        {
            return;
        }

        // Spawn the effect as a child of the character.
        GameObject effect = Instantiate(data.vfxPrefab, effectAnchor);

        effect.transform.localPosition = Vector3.zero;

        effect.transform.localRotation = Quaternion.identity;

        // Store the effect for later removal.
        activeVFX.Add(data, effect);
    }

    // PURPOSE:
    // Removes the visual effect when its status expires.
    public void StopEffect(StatusEffectData data)
    {
        if (data == null)
        {
            return;
        }

        if (!activeVFX.TryGetValue(data, out GameObject effect))
        {
            return;
        }

        if (effect != null)
        {
            Destroy(effect);
        }

        activeVFX.Remove(data);
    }

    // PURPOSE:
    // Removes all active status VFX.
    // Used when effects are cleared or the Entity dies.
    public void StopAllEffects()
    {
        foreach (GameObject effect in activeVFX.Values)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }

        activeVFX.Clear();
    }
    private void OnDisable()
    {
        StopAllEffects();
    }
}
