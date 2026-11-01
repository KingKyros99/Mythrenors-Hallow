using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MythrenorHallow.UI
{
    public class ResourceBar : MonoBehaviour
    {
        [Header("Images")]
        [SerializeField, Tooltip("The coloured bar. Needs a sprite so the Filled type works.")]
        private Image fill;
        [SerializeField, Tooltip("The pale bar behind the fill that slides down after damage. Place it BEHIND the fill.")]
        private Image trail;
        [SerializeField, Tooltip("Optional text such as '120 / 150'.")]
        private TMP_Text valueText;

        [Header("Motion (fractions of the bar per second)")]
        [SerializeField, Tooltip("How quickly the fill drops when damaged.")]
        private float dropSpeed = 4f;
        [SerializeField, Tooltip("How quickly the fill rises when healed. Lower is smoother.")]
        private float riseSpeed = 1.2f;
        [SerializeField, Tooltip("Seconds the trail waits before sliding after damage.")]
        private float trailDelay = 0.4f;
        [SerializeField, Tooltip("How quickly the trail catches up to the fill.")]
        private float trailSpeed = 0.8f;

        private RectTransform rectTransform;
        private float fillValue = 1f;
        private float trailValue = 1f;
        private float targetValue = 1f;
        private float trailTimer;
        private bool animating;
        private int lastCurrent = -1;
        private int lastMax = -1;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;

            if (fill == null)
            {
                Debug.LogWarning("ResourceBar: 'Fill' image is not assigned.", this);
                return;
            }

            
            ConfigureFilled(fill);
            if (trail != null) ConfigureFilled(trail);
            Apply();
        }

        private static void ConfigureFilled(Image image)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        
        public void SetValue(float current, float max)
        {
            
            if (max <= 0f)
            {
                Debug.LogWarning("ResourceBar.SetValue: max must be greater than 0.", this);
                return;
            }

            float target = Mathf.Clamp01(current / max);

            
            if (target < targetValue) trailTimer = trailDelay;

            targetValue = target;
            UpdateText(current, max);
            animating = true;
        }

        
        public void SetInstant(float current, float max)
        {
            if (max <= 0f)
            {
                Debug.LogWarning("ResourceBar.SetInstant: max must be greater than 0.", this);
                return;
            }

            targetValue = Mathf.Clamp01(current / max);
            fillValue = targetValue;
            trailValue = targetValue;
            trailTimer = 0f;
            animating = false;
            UpdateText(current, max);
            Apply();
        }

        
        public void SetWidth(float width)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        private void Update()
        {
            
            if (!animating || fill == null) return;

            float dt = Time.unscaledDeltaTime;

            
            float speed = fillValue > targetValue ? dropSpeed : riseSpeed;
            fillValue = Mathf.MoveTowards(fillValue, targetValue, speed * dt);

            if (trailValue < fillValue)
            {
               
                trailValue = fillValue;
            }
            else if (trailValue > fillValue)
            {
                
                if (trailTimer > 0f) trailTimer -= dt;
                else trailValue = Mathf.MoveTowards(trailValue, fillValue, trailSpeed * dt);
            }

            Apply();

            animating = !Mathf.Approximately(fillValue, targetValue)
                        || !Mathf.Approximately(trailValue, fillValue);
        }

        private void Apply()
        {
            fill.fillAmount = fillValue;
            if (trail != null) trail.fillAmount = trailValue;
        }

        private void UpdateText(float current, float max)
        {
            if (valueText == null) return;

            int c = Mathf.CeilToInt(current);
            int m = Mathf.CeilToInt(max);

            // dont rebuild the text if the numbers are the same
            if (c == lastCurrent && m == lastMax) return;

            lastCurrent = c;
            lastMax = m;
            valueText.text = c + " / " + m;
        }
    }
}