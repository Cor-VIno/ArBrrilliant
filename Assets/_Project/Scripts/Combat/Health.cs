using System;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0f;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            maxHealth = Mathf.Max(1f, maxHealth);

            if (currentHealth <= 0f || currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (IsDead)
            {
                Kill();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Kill()
        {
            if (currentHealth != 0f)
            {
                currentHealth = 0f;
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }

            OnDied?.Invoke();
        }
    }
}
