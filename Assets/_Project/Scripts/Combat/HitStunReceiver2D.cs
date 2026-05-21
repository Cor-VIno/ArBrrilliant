using System;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class HitStunReceiver2D : MonoBehaviour
    {
        [SerializeField] private bool logHitStun = false;

        private bool isStunned;
        private float remainingTime;

        public bool IsStunned => isStunned;

        public event Action<float> OnHitStunStarted;
        public event Action<float> OnHitStunRefreshed;
        public event Action OnHitStunEnded;

        public void ApplyHitStun(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            if (!isStunned)
            {
                isStunned = true;
                remainingTime = duration;
                OnHitStunStarted?.Invoke(duration);

                if (logHitStun)
                {
                    Debug.Log($"{name} entered hit stun for {duration:0.###}s.", this);
                }

                return;
            }

            remainingTime = Mathf.Max(remainingTime, duration);
            OnHitStunRefreshed?.Invoke(remainingTime);

            if (logHitStun)
            {
                Debug.Log($"{name} refreshed hit stun to {remainingTime:0.###}s.", this);
            }
        }

        private void Update()
        {
            if (!isStunned)
            {
                return;
            }

            remainingTime -= Time.deltaTime;

            if (remainingTime > 0f)
            {
                return;
            }

            isStunned = false;
            remainingTime = 0f;
            OnHitStunEnded?.Invoke();

            if (logHitStun)
            {
                Debug.Log($"{name} ended hit stun.", this);
            }
        }

        private void OnDisable()
        {
            isStunned = false;
            remainingTime = 0f;
        }
    }
}
