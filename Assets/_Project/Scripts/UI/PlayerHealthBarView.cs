using JingHongLu.Combat;
using TMPro;
using UnityEngine;
using PlayerInputReader = JingHongLu.Input.PlayerInputReader;

namespace JingHongLu.UI
{
    public sealed class PlayerHealthBarView : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform healthFill;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private bool hideWhenDead;
        [SerializeField] private bool showValueText = true;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void ResolveReferences()
        {
            if (health == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    health = player.GetComponent<Health>();
                }
            }

            if (health == null)
            {
                PlayerInputReader playerInput = FindAnyObjectByType<PlayerInputReader>();
                if (playerInput != null)
                {
                    health = playerInput.GetComponent<Health>();
                }
            }

            if (root == null)
            {
                root = gameObject;
            }
        }

        private void Subscribe()
        {
            if (health == null)
            {
                Debug.LogWarning("[PlayerHealthBar] Health is not bound.", this);
                Refresh(0f, 1f);
                return;
            }

            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDied -= HandleDied;
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDied += HandleDied;
        }

        private void Unsubscribe()
        {
            if (health == null)
            {
                return;
            }

            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDied -= HandleDied;
        }

        private void HandleHealthChanged(float current, float max)
        {
            Refresh(current, max);
        }

        private void HandleDied()
        {
            Refresh(0f, health != null ? health.MaxHealth : 1f);
        }

        private void Refresh()
        {
            if (health == null)
            {
                Refresh(0f, 1f);
                return;
            }

            Refresh(health.CurrentHealth, health.MaxHealth);
        }

        private void Refresh(float current, float max)
        {
            float normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            bool isAlive = current > 0f;

            SetHorizontalFill(healthFill, normalized);

            if (valueText != null)
            {
                valueText.gameObject.SetActive(showValueText && (!hideWhenDead || isAlive));
                valueText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }

            if (root != null)
            {
                root.SetActive(!hideWhenDead || isAlive);
            }
        }

        private static void SetHorizontalFill(RectTransform fill, float normalized)
        {
            if (fill == null)
            {
                return;
            }

            normalized = Mathf.Clamp01(normalized);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(normalized, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }
    }
}
