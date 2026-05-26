using System;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class ShieldComponent : MonoBehaviour
    {
        [SerializeField] private float maxShield = 30f;
        [SerializeField] private float currentShield;
        [SerializeField] private bool startFull = true;
        [SerializeField] private bool blockHealthDamageWhileShielded = true;
        [SerializeField] private float healthDamageMultiplierWhileShielded = 0f;
        [SerializeField] private bool logShield = true;

        public event Action<float, float> OnShieldChanged;

        // NEW
        public event Action<float> OnShieldDamaged;
        public event Action OnShieldBlocked;

        public event Action OnShieldBroken;

        public float CurrentShield => currentShield;
        public float MaxShield => maxShield;
        public bool HasShield => currentShield > 0f;
        public bool IsBroken => currentShield <= 0f;

        public bool BlockHealthDamageWhileShielded =>
            blockHealthDamageWhileShielded;

        public float HealthDamageMultiplierWhileShielded =>
            Mathf.Max(0f, healthDamageMultiplierWhileShielded);

        private void Awake()
        {
            Initialize();
        }

        private void OnValidate()
        {
            maxShield = Mathf.Max(0f, maxShield);

            currentShield =
                Mathf.Clamp(currentShield, 0f, maxShield);

            healthDamageMultiplierWhileShielded =
                Mathf.Max(0f, healthDamageMultiplierWhileShielded);
        }

        public void Initialize()
        {
            maxShield = Mathf.Max(0f, maxShield);

            if (startFull)
            {
                currentShield = maxShield;
            }
            else
            {
                currentShield =
                    Mathf.Clamp(currentShield, 0f, maxShield);
            }

            OnShieldChanged?.Invoke(
                currentShield,
                maxShield);
        }

        public float ApplyShieldDamage(float amount)
        {
            if (amount <= 0f || currentShield <= 0f)
            {
                return 0f;
            }

            float oldShield = currentShield;

            currentShield =
                Mathf.Max(0f, currentShield - amount);

            float appliedAmount =
                oldShield - currentShield;

            // NEW
            OnShieldDamaged?.Invoke(appliedAmount);

            OnShieldChanged?.Invoke(
                currentShield,
                maxShield);

            if (logShield)
            {
                Debug.Log(
                    $"[Shield] Took shield damage: {appliedAmount}. Current={currentShield}/{maxShield}",
                    this);
            }

            if (oldShield > 0f &&
                currentShield <= 0f)
            {
                if (logShield)
                {
                    Debug.Log(
                        "[Shield] Broken.",
                        this);
                }

                OnShieldBroken?.Invoke();
            }

            return appliedAmount;
        }

        // NEW
        public void NotifyBlocked()
        {
            OnShieldBlocked?.Invoke();
        }
    }
}