using System;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class AirborneTarget2D : MonoBehaviour
    {
        [SerializeField] private bool logStateChange = false;

        private float remainingTime;

        public event Action<AirborneTarget2D> OnAirborneStarted;
        public event Action<AirborneTarget2D> OnAirborneRefreshed;
        public event Action<AirborneTarget2D> OnAirborneEnded;

        public bool IsAirborne => remainingTime > 0f;
        public float RemainingTime => remainingTime;

        private void Update()
        {
            if (remainingTime <= 0f)
            {
                return;
            }

            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                OnAirborneEnded?.Invoke(this);

                if (logStateChange)
                {
                    Debug.Log($"{name} ended airborne state.", this);
                }
            }
        }

        public void MarkAirborne(float duration)
        {
            float safeDuration = Mathf.Max(0f, duration);

            if (safeDuration <= 0f)
            {
                return;
            }

            bool wasAirborne = IsAirborne;
            remainingTime = Mathf.Max(remainingTime, safeDuration);

            if (!wasAirborne)
            {
                OnAirborneStarted?.Invoke(this);

                if (logStateChange)
                {
                    Debug.Log($"{name} entered airborne state.", this);
                }

                return;
            }

            OnAirborneRefreshed?.Invoke(this);

            if (logStateChange)
            {
                Debug.Log($"{name} refreshed airborne state.", this);
            }
        }

        public void ClearAirborne()
        {
            bool wasAirborne = IsAirborne;
            remainingTime = 0f;

            if (wasAirborne)
            {
                OnAirborneEnded?.Invoke(this);
            }

            if (logStateChange)
            {
                Debug.Log($"{name} cleared airborne state.", this);
            }
        }
    }
}
