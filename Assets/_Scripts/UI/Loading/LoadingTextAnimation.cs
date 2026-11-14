using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LoadingTextAnimation : MonoBehaviour
{
    [SerializeField] private float dotInterval = 0.4f;
    [SerializeField]
    private Color textColor =
        new Color32(0x70, 0xFF, 0xFF, 0xFF);

    private TMP_Text label;
    private float elapsed;
    private int previousDots = -1;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        elapsed = 0f;
        previousDots = -1;

        label.richText = true;
        label.enableAutoSizing = false;
        label.color = textColor;

        UpdateDots(1);
    }

    private void LateUpdate()
    {
        elapsed += Time.unscaledDeltaTime;

        int dots = Mathf.FloorToInt(
            elapsed / Mathf.Max(0.1f, dotInterval)
        ) % 3 + 1;

        if (dots != previousDots)
            UpdateDots(dots);
    }

    private void UpdateDots(int dots)
    {
        label.text =
            "Loading<space=0.15em><nobr><alpha=" +
            (dots >= 1 ? "#FF" : "#00") +
            ">.<alpha=" +
            (dots >= 2 ? "#FF" : "#00") +
            ">.<alpha=" +
            (dots >= 3 ? "#FF" : "#00") +
            ">.</nobr>";

        previousDots = dots;
    }
}