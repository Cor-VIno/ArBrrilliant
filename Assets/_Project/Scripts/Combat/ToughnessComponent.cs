using System;
using System.Collections;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class ToughnessComponent : MonoBehaviour
    {
        [SerializeField] private float maxToughness = 100f;
        [SerializeField] private float currentToughness = 100f;
        [SerializeField] private float breakDuration = 2f;
        [SerializeField] private bool startFull = true;
        [SerializeField] private bool recoverFullAfterBreak = true;
        [SerializeField] private bool logToughness;

        private Coroutine breakRoutine;
        private bool isBroken;

        public event Action<float, float> OnToughnessChanged;
        public event Action OnBroken;
        public event Action OnBreakRecovered;

        public float CurrentToughness => currentToughness;
        public float MaxToughness => maxToughness;
        public bool HasToughness => maxToughness > 0f && currentToughness > 0f;
        public bool IsBroken => isBroken;
        public bool HasActiveArmor => HasToughness && !isBroken;

        private void Awake()
        {
            maxToughness = Mathf.Max(0f, maxToughness);

            if (startFull)
            {
                currentToughness = maxToughness;
            }
            else
            {
                currentToughness = Mathf.Clamp(currentToughness, 0f, maxToughness);
            }
        }

        private void OnEnable()
        {
            OnToughnessChanged?.Invoke(currentToughness, maxToughness);
        }

        private void OnDisable()
        {
            if (breakRoutine != null)
            {
                StopCoroutine(breakRoutine);
                breakRoutine = null;
            }
        }

        public void ApplyToughnessDamage(float amount)
        {
            if (amount <= 0f || isBroken || currentToughness <= 0f)
            {
                return;
            }

            currentToughness = Mathf.Max(0f, currentToughness - amount);
            OnToughnessChanged?.Invoke(currentToughness, maxToughness);

            if (logToughness)
            {
                Debug.Log(
                    $"[Toughness] Took toughness damage: {amount}. Current={currentToughness}/{maxToughness}",
                    this);
            }

            if (currentToughness <= 0f)
            {
                ForceBreak();
            }
        }

        public void ForceBreak()
        {
            if (isBroken)
            {
                return;
            }

            isBroken = true;
            currentToughness = 0f;
            OnToughnessChanged?.Invoke(currentToughness, maxToughness);
            OnBroken?.Invoke();

            if (logToughness)
            {
                Debug.Log("[Toughness] Broken.", this);
            }

            if (breakRoutine != null)
            {
                StopCoroutine(breakRoutine);
            }

            breakRoutine = StartCoroutine(BreakRoutine());
        }

        public void RecoverToughness()
        {
            if (breakRoutine != null)
            {
                StopCoroutine(breakRoutine);
                breakRoutine = null;
            }

            isBroken = false;

            if (recoverFullAfterBreak)
            {
                currentToughness = maxToughness;
            }
            else
            {
                currentToughness = Mathf.Clamp(currentToughness, 0f, maxToughness);
            }

            OnToughnessChanged?.Invoke(currentToughness, maxToughness);
            OnBreakRecovered?.Invoke();

            if (logToughness)
            {
                Debug.Log("[Toughness] Break recovered.", this);
            }
        }

        private IEnumerator BreakRoutine()
        {
            float duration = Mathf.Max(0f, breakDuration);

            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            breakRoutine = null;
            RecoverToughness();
        }
    }
}
