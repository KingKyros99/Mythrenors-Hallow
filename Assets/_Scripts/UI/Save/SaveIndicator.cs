using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SaveIndicator : MonoBehaviour
{
    public float showTime = 3f;
    public float fadeSpeed = 3f;

    private CanvasGroup group;
    private float timer;
    private bool hooked;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
    }

    private void OnDisable()
    {
        if (hooked && SaveManager.Instance != null)
            SaveManager.Instance.Saved -= Show;

        hooked = false;
    }

    private void Show()
    {
        timer = showTime;
    }

    private void Update()
    {
        if (!hooked && SaveManager.Instance != null)
        {
            SaveManager.Instance.Saved += Show;
            hooked = true;
        }

        timer -= Time.unscaledDeltaTime;

        float target = timer > 0f ? 1f : 0f;

        group.alpha = Mathf.MoveTowards(
            group.alpha,
            target,
            fadeSpeed * Time.unscaledDeltaTime
        );
    }
}