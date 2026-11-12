using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MythrenorHallow.UI
{
    public class ResourceBar : MonoBehaviour
    {
        [Header("Images")]
        [SerializeField] private Image fill;
        [SerializeField] private Image trail;
        [SerializeField] private TMP_Text valueText;

        [Header("Direction")]
        [SerializeField] private bool vertical;

        [Header("Motion (fractions of the bar per second)")]
        [SerializeField] private float dropSpeed = 4f;
        [SerializeField] private float riseSpeed = 1.2f;
        [SerializeField] private float trailDelay = 0.4f;
        [SerializeField] private float trailSpeed = 0.8f;

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

            if (fill == null) return;

            ConfigureFilled(fill);

            if (trail != null)
                ConfigureFilled(trail);

            Apply();
        }

        private void ConfigureFilled(Image image)
        {
            image.type = Image.Type.Filled;

            if (vertical)
            {
                image.fillMethod = Image.FillMethod.Vertical;
                image.fillOrigin = (int)Image.OriginVertical.Bottom;
            }
            else
            {
                image.fillMethod = Image.FillMethod.Horizontal;
                image.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

        public void SetValue(float current, float max)
        {
            if (max <= 0f) return;

            float target = Mathf.Clamp01(current / max);

            if (target < targetValue)
                trailTimer = trailDelay;

            targetValue = target;
            UpdateText(current, max);
            animating = true;
        }

        public void SetInstant(float current, float max)
        {
            if (max <= 0f) return;

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
            if (rectTransform == null)
                rectTransform = transform as RectTransform;

            if (rectTransform == null) return;

            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width
            );
        }

        private void Update()
        {
            if (!animating || fill == null) return;

            float deltaTime = Time.unscaledDeltaTime;

            float speed = fillValue > targetValue
                ? dropSpeed
                : riseSpeed;

            fillValue = Mathf.MoveTowards(
                fillValue,
                targetValue,
                speed * deltaTime
            );

            if (trailValue < fillValue)
            {
                trailValue = fillValue;
            }
            else if (trailValue > fillValue)
            {
                if (trailTimer > 0f)
                {
                    trailTimer -= deltaTime;
                }
                else
                {
                    trailValue = Mathf.MoveTowards(
                        trailValue,
                        fillValue,
                        trailSpeed * deltaTime
                    );
                }
            }

            Apply();

            animating =
                !Mathf.Approximately(fillValue, targetValue) ||
                !Mathf.Approximately(trailValue, fillValue);
        }

        private void Apply()
        {
            if (fill != null)
                fill.fillAmount = fillValue;

            if (trail != null)
                trail.fillAmount = trailValue;
        }

        private void UpdateText(float current, float max)
        {
            if (valueText == null) return;

            int currentValue = Mathf.CeilToInt(current);
            int maxValue = Mathf.CeilToInt(max);

            if (currentValue == lastCurrent && maxValue == lastMax)
                return;

            lastCurrent = currentValue;
            lastMax = maxValue;

            valueText.text = currentValue + " / " + maxValue;
        }
    }
}