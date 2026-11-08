using UnityEngine;
using MythrenorHallow.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public ResourceBar bar;

    CharacterBase character;
    Camera cam;

    void Awake()
    {
        character = GetComponentInParent<CharacterBase>();
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null) transform.forward = cam.transform.forward;

        if (character == null || bar == null) return;
        bar.SetValue(character.CurrentHealth, character.MaxHealth);
        if (character.IsDead) gameObject.SetActive(false);
    }
}