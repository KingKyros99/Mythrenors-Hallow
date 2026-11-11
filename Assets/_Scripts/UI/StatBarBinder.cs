using MythrenorHallow.UI;
using UnityEngine;

public class StatBarBinder : MonoBehaviour
{
    [SerializeField] CharacterBase character;
    [SerializeField] ResourceBar health;
    [SerializeField] ResourceBar mana;
    [SerializeField] ResourceBar stamina;
    [SerializeField] GameObject hideWhenDead;

    void Awake()
    {
        if (character == null) character = GetComponentInParent<CharacterBase>();
        if (character == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) character = p.GetComponent<CharacterBase>();
        }
    }

    void Update()
    {
        if (character == null) return;
        if (health != null) health.SetValue(character.CurrentHealth, character.MaxHealth);
        if (mana != null) mana.SetValue(character.CurrentMana, character.MaxMana);
        if (stamina != null) stamina.SetValue(character.CurrentStamina, character.MaxStamina);
        if (hideWhenDead != null) hideWhenDead.SetActive(!character.IsDead);
    }
}