using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class KeyCounterUI : MonoBehaviour
{
    [SerializeField] private PlayerInteractions playerInteractions;

    private TMP_Text label;
    private int displayedCount = -1;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        displayedCount = -1;
    }

    private void LateUpdate()
    {
        int count = playerInteractions != null
            ? playerInteractions.Keys.Count
            : 0;

        if (count == displayedCount)
            return;

        displayedCount = count;
        label.SetText("Keys: {0}", count);
    }
}