using UnityEngine;
using TMPro;

namespace MythrenorHallow.UI
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private CharacterBase boss;
        [SerializeField] private ResourceBar bar;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private string bossName = "Goblin Leader";
        [SerializeField] private Transform distanceTarget;
        [SerializeField] private float showDistance = 25f;

        private Transform player;

        private void Start()
        {
            if (nameText != null) nameText.text = bossName;
            if (root != null) root.SetActive(false);
        }

        private void Update()
        {
            if (boss == null || bar == null || root == null) return;

            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }

            Transform target = distanceTarget != null ? distanceTarget : boss.transform;
            float dist = player != null ? Vector3.Distance(player.position, target.position) : -1f;
            bool show = !boss.IsDead && player != null && dist <= showDistance;

            if (root.activeSelf != show) root.SetActive(show);
            if (show) bar.SetValue(boss.CurrentHealth, boss.MaxHealth);
        }
    }
}