using System;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class PerfectDodgeSlowMotionController : MonoBehaviour
    {
        [SerializeField] private float slowDuration = 1.5f;
        [SerializeField] private float enemyTimeScale = 0.2f;
        [SerializeField] private float projectileTimeScale = 0.2f;
        [SerializeField] private float environmentTimeScale = 0.2f;
        [SerializeField] private bool logPerfectDodgeSlow = true;

        private float remainingTime;

        public static PerfectDodgeSlowMotionController Instance { get; private set; }

        public event Action OnPerfectDodgeSlowStarted;
        public event Action OnPerfectDodgeSlowEnded;

        public bool IsActive => IsPerfectDodgeSlowActive;
        public bool IsPerfectDodgeSlowActive => remainingTime > 0f;
        public float EnemyTimeScale => IsPerfectDodgeSlowActive ? Mathf.Clamp01(enemyTimeScale) : 1f;
        public float ProjectileTimeScale => IsPerfectDodgeSlowActive ? Mathf.Clamp01(projectileTimeScale) : 1f;
        public float EnvironmentTimeScale => IsPerfectDodgeSlowActive ? Mathf.Clamp01(environmentTimeScale) : 1f;

        private void Awake()
        {
            if (Instance == null || Instance == this)
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Update()
        {
            if (!IsPerfectDodgeSlowActive)
            {
                return;
            }

            remainingTime -= Time.unscaledDeltaTime;

            if (remainingTime <= 0f)
            {
                EndSlow(wasActive: true);
            }
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (IsPerfectDodgeSlowActive)
            {
                EndSlow(wasActive: true);
            }
        }

        public void TriggerSlowMotion()
        {
            StartPerfectDodgeSlow();
        }

        public void StartPerfectDodgeSlow()
        {
            bool wasActive = IsPerfectDodgeSlowActive;
            remainingTime = Mathf.Max(0f, slowDuration);

            if (!wasActive)
            {
                OnPerfectDodgeSlowStarted?.Invoke();
            }

            if (logPerfectDodgeSlow)
            {
                Debug.Log(
                    $"[PerfectDodgeSlow] Started. Duration={slowDuration}, EnemyScale={EnemyTimeScale}, ProjectileScale={ProjectileTimeScale}",
                    this);
            }

            if (remainingTime <= 0f)
            {
                EndSlow(wasActive: wasActive);
            }
        }

        public void CancelSlowMotion()
        {
            CancelPerfectDodgeSlow();
        }

        public void CancelPerfectDodgeSlow()
        {
            if (!IsPerfectDodgeSlowActive)
            {
                return;
            }

            remainingTime = 0f;
            EndSlow(wasActive: true);
        }

        private void EndSlow(bool wasActive)
        {
            remainingTime = 0f;

            if (wasActive)
            {
                OnPerfectDodgeSlowEnded?.Invoke();
            }

            if (logPerfectDodgeSlow)
            {
                Debug.Log("[PerfectDodgeSlow] Ended.", this);
            }
        }
    }
}
