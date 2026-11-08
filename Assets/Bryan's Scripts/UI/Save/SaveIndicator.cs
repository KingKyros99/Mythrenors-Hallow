using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class SaveIndicator : MonoBehaviour
{
    public float showTime = 3f;
    public float fadeSpeed = 3f;

    CanvasGroup group;
    float timer;
    bool hooked;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        Debug.Log("[Save] Indicator awake on " + gameObject.name);
    }

    void OnDisable()
    {
        if (hooked && SaveManager.Instance != null) SaveManager.Instance.Saved -= Show;
        hooked = false;
    }

    void Show()
    {
        Debug.Log("[Save] Indicator Show called");
        timer = showTime;
    }

    void Update()
    {
        if (!hooked && SaveManager.Instance != null)
        {
            SaveManager.Instance.Saved += Show;
            hooked = true;
            Debug.Log("[Save] Indicator hooked to SaveManager");
        }

        // TEST KEY: press F9 to force the text to show. Remove after testing.
        if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame) Show();

        timer -= Time.unscaledDeltaTime;
        float target = timer > 0f ? 1f : 0f;
        group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
    }
}