using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SaveIndicator : MonoBehaviour
{
    public float showTime = 1.5f;
    public float fadeSpeed = 3f;

    CanvasGroup group;
    float timer;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
    }

    void OnEnable()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.Saved += Show;
    }

    void OnDisable()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.Saved -= Show;
    }

    void Show() { timer = showTime; }

    void Update()
    {
        timer -= Time.unscaledDeltaTime;
        float target = timer > 0f ? 1f : 0f;
        group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
    }
}