using UnityEngine;
using UnityEngine.UI;

public class TitleMenuSelector : MonoBehaviour
{
    [SerializeField] private RectTransform crystal;
    [SerializeField] private RectTransform line;
    [SerializeField] private Button[] buttons;
    [SerializeField] private float slideSpeed = 14f;
    [SerializeField] private float lineOffsetY = -20f;

    private int index = -1;

    private void OnEnable() { SelectFirst(); }

    public void SelectFirst()
    {
        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i] != null && buttons[i].interactable) { Select(i); return; }
    }

    public void Select(int i)
    {
        if (i < 0 || i >= buttons.Length || !buttons[i].interactable) return;
        index = i;
    }

    private void Update()
    {
        if (index < 0 || index >= buttons.Length) return;
        Vector3 p = buttons[index].transform.position;
        float t = 1f - Mathf.Exp(-slideSpeed * Time.unscaledDeltaTime);
        if (crystal != null)
            crystal.position = new Vector3(crystal.position.x, Mathf.Lerp(crystal.position.y, p.y, t), crystal.position.z);
        if (line != null)
            line.position = new Vector3(line.position.x, Mathf.Lerp(line.position.y, p.y + lineOffsetY, t), line.position.z);
    }
}