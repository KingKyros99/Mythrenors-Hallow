using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// MYTHRENOR'S HALLOW - DAMAGE FLASH
//
// PURPOSE:
// Briefly changes a character's appearance
// when damage is received.
//
// NOTE:
// Reusable for both player and enemy entities.
// Does not modify CharacterBase or damage calculations.

public class DamageFlash : MonoBehaviour
{
    // Damage Flash Settings
    [SerializeField] private Color flashColor = Color.red;

    [SerializeField, Min(0.01f)]
    private float flashDuration = 0.15f;

    // Stores which materials we can flash.
    private struct FlashSlot
    {
        public Renderer renderer;
        public int materialIndex;
        public int colorProperty;
        public Color originalColor;
    }

    private readonly List<FlashSlot> flashSlots =
        new List<FlashSlot>();

    private Coroutine flashCoroutine;

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    private static readonly int StandardColor = Shader.PropertyToID("_Color");

    private void Awake()
    {
        // Find the character's visible mesh renderers.
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer))
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];

                if (material == null)
                {
                    continue;
                }

                int property;

                if (material.HasProperty(BaseColor))
                {
                    property = BaseColor;
                }
                else if (material.HasProperty(StandardColor))
                {
                    property = StandardColor;
                }
                else
                {
                    continue;
                }

                flashSlots.Add(new FlashSlot{renderer = renderer, materialIndex = i, colorProperty = property, originalColor = material.GetColor(property)});
            }
        }
    }

    // PURPOSE:
    // Starts or restarts the damage flash.
    public void PlayFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        SetFlashColor(true);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        yield return new WaitForSeconds(flashDuration);

        SetFlashColor(false);

        flashCoroutine = null;
    }

    // PURPOSE:
    // Changes the renderer color without editing
    // the original shared materials.
    private void SetFlashColor(bool flashing)
    {
        foreach (FlashSlot slot in flashSlots)
        {
            if (slot.renderer == null)
            {
                continue;
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();

            slot.renderer.GetPropertyBlock(block, slot.materialIndex);

            block.SetColor(slot.colorProperty, flashing ? flashColor : slot.originalColor);

            slot.renderer.SetPropertyBlock(block, slot.materialIndex);
        }
    }

    private void OnDisable()
    {
        SetFlashColor(false);
    }

    // PURPOSE:
    // Lets us test the flash before connecting damage.
    [ContextMenu("Test Damage Flash")]
    private void TestDamageFlash()
    {
        if (Application.isPlaying)
        {
            PlayFlash();
        }
    }
}
