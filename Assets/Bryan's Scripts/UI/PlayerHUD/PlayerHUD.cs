using UnityEngine;
using UnityEngine.UI;

namespace MythrenorHallow.UI
{
    
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Bars")]
        [SerializeField, Tooltip("The health ResourceBar.")]
        private ResourceBar healthBar;
        [SerializeField, Tooltip("The mana ResourceBar.")]
        private ResourceBar manaBar;
        [SerializeField, Tooltip("The stamina ResourceBar.")]
        private ResourceBar staminaBar;
        [SerializeField, Tooltip("CanvasGroup on the health bar's root. It is pulsed when health is low.")]
        private CanvasGroup healthBarGroup;

        [Header("Low health")]
        [SerializeField, Range(0.05f, 0.5f), Tooltip("Below this fraction of max health, the warning effects start.")]
        private float lowHealthThreshold = 0.25f;
        [SerializeField, Tooltip("Pulse speed. Lower is slower.")]
        private float pulseSpeed = 2f;
        [SerializeField, Range(0f, 0.8f), Tooltip("How much the health bar dims at the bottom of each pulse.")]
        private float pulseDepth = 0.35f;
        [SerializeField, Tooltip("Full-screen image with a red edge glow. Its centre must be transparent.")]
        private Image lowHealthVignette;
        [SerializeField, Range(0f, 1f), Tooltip("Strongest alpha of the vignette.")]
        private float vignetteMaxAlpha = 0.45f;
        [SerializeField, Tooltip("How quickly the warning fades in and out.")]
        private float fadeSpeed = 2f;

        [Header("Bar length")]
        [SerializeField, Tooltip("If on, the health bar gets longer as max health grows.")]
        private bool growWithMaxHealth = true;
        [SerializeField, Tooltip("Pixels of bar length per point of max health.")]
        private float pixelsPerPoint = 2f;
        [SerializeField, Tooltip("Shortest the health bar can be, in pixels.")]
        private float minWidth = 300f;
        [SerializeField, Tooltip("Longest the health bar can be, in pixels.")]
        private float maxWidth = 700f;

        private bool isLow;
        private float lowAmount; 
        private float lastMaxHealth = -1f;

        private void Awake()
        {
            if (healthBar == null) Debug.LogWarning("PlayerHUD: Health Bar is not assigned.", this);
            if (manaBar == null) Debug.LogWarning("PlayerHUD: Mana Bar is not assigned.", this);
            if (staminaBar == null) Debug.LogWarning("PlayerHUD: Stamina Bar is not assigned.", this);

            
            if (lowHealthVignette != null) lowHealthVignette.raycastTarget = false;
        }

        
        public void SetHealth(float current, float max)
        {
            
            if (max <= 0f)
            {
                Debug.LogWarning("PlayerHUD.SetHealth: max must be greater than 0.", this);
                return;
            }

            if (healthBar != null) healthBar.SetValue(current, max);

            
            isLow = current > 0f && current / max <= lowHealthThreshold;
            ResizeHealthBar(max);
        }

        
        public void SetMana(float current, float max)
        {
            if (max <= 0f)
            {
                Debug.LogWarning("PlayerHUD.SetMana: max must be greater than 0.", this);
                return;
            }

            if (manaBar != null) manaBar.SetValue(current, max);
        }

        
        public void SetStamina(float current, float max)
        {
            if (max <= 0f)
            {
                Debug.LogWarning("PlayerHUD.SetStamina: max must be greater than 0.", this);
                return;
            }

            if (staminaBar != null) staminaBar.SetValue(current, max);
        }

        private void ResizeHealthBar(float max)
        {
            if (!growWithMaxHealth || healthBar == null) return;

            
            if (Mathf.Approximately(max, lastMaxHealth)) return;

            lastMaxHealth = max;
            healthBar.SetWidth(Mathf.Clamp(max * pixelsPerPoint, minWidth, maxWidth));
        }

        private void Update()
        {
            
            if (!isLow && lowAmount <= 0f) return;

            lowAmount = Mathf.MoveTowards(lowAmount, isLow ? 1f : 0f, fadeSpeed * Time.unscaledDeltaTime);

            
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);

            if (healthBarGroup != null)
                healthBarGroup.alpha = 1f - lowAmount * pulseDepth * pulse;

            if (lowHealthVignette != null)
            {
                Color c = lowHealthVignette.color;
                c.a = lowAmount * vignetteMaxAlpha * (0.6f + 0.4f * pulse);
                lowHealthVignette.color = c;
            }
        }
    }
}