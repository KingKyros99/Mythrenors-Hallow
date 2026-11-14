using UnityEngine;
using UnityEngine.UI;

namespace MythrenorHallow.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Bars")]
        [SerializeField] private ResourceBar healthBar;
        [SerializeField] private ResourceBar manaBar;
        [SerializeField] private ResourceBar staminaBar;
        [SerializeField] private CanvasGroup healthBarGroup;

        [Header("Low health")]
        [SerializeField, Range(0.05f, 0.5f)]
        private float lowHealthThreshold = 0.25f;

        [SerializeField] private float pulseSpeed = 2f;

        [SerializeField, Range(0f, 0.8f)]
        private float pulseDepth = 0.35f;

        [SerializeField] private Image lowHealthVignette;

        [SerializeField, Range(0f, 1f)]
        private float vignetteMaxAlpha = 0.45f;

        [SerializeField] private float fadeSpeed = 2f;

        [Header("Bar length")]
        [SerializeField] private bool growWithMaxHealth = true;
        [SerializeField] private float pixelsPerPoint = 2f;
        [SerializeField] private float minWidth = 300f;
        [SerializeField] private float maxWidth = 700f;

        private bool isLow;
        private float lowAmount;
        private float lastMaxHealth = -1f;

        private void Awake()
        {
            if (lowHealthVignette != null)
                lowHealthVignette.raycastTarget = false;
        }

        public void SetHealth(float current, float max)
        {
            if (max <= 0f) return;

            if (healthBar != null)
                healthBar.SetValue(current, max);

            isLow = current > 0f &&
                    current / max <= lowHealthThreshold;

            ResizeHealthBar(max);
        }

        public void SetMana(float current, float max)
        {
            if (max <= 0f) return;

            if (manaBar != null)
                manaBar.SetValue(current, max);
        }

        public void SetStamina(float current, float max)
        {
            if (max <= 0f) return;

            if (staminaBar != null)
                staminaBar.SetValue(current, max);
        }

        private void ResizeHealthBar(float max)
        {
            if (!growWithMaxHealth || healthBar == null)
                return;

            if (Mathf.Approximately(max, lastMaxHealth))
                return;

            lastMaxHealth = max;

            healthBar.SetWidth(
                Mathf.Clamp(
                    max * pixelsPerPoint,
                    minWidth,
                    maxWidth
                )
            );
        }

        private void Update()
        {
            if (!isLow && lowAmount <= 0f) return;

            lowAmount = Mathf.MoveTowards(
                lowAmount,
                isLow ? 1f : 0f,
                fadeSpeed * Time.unscaledDeltaTime
            );

            float pulse =
                0.5f + 0.5f * Mathf.Sin(
                    Time.unscaledTime * pulseSpeed
                );

            if (healthBarGroup != null)
            {
                healthBarGroup.alpha =
                    1f - lowAmount * pulseDepth * pulse;
            }

            if (lowHealthVignette != null)
            {
                Color color = lowHealthVignette.color;

                color.a =
                    lowAmount *
                    vignetteMaxAlpha *
                    (0.6f + 0.4f * pulse);

                lowHealthVignette.color = color;
            }
        }
    }
}