using System.Collections;
using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerPerfectDodgeEffectBinder : MonoBehaviour
    {
        [SerializeField] private PlayerPerfectDodgeController2D perfectDodgeController;
        [SerializeField] private PlayerDashController2D dashController;
        [SerializeField] private PerfectDodgeSlowMotionController slowMotionController;
        [SerializeField] private Damageable damageable;
        [SerializeField] private float extraInvincibleDuration = 0.1f;
        [SerializeField] private bool logPerfectDodgeEffect = true;

        private Coroutine extraInvincibleRoutine;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (perfectDodgeController != null)
            {
                perfectDodgeController.OnPerfectDodgeTriggered += HandlePerfectDodgeTriggered;
            }
        }

        private void OnDisable()
        {
            if (perfectDodgeController != null)
            {
                perfectDodgeController.OnPerfectDodgeTriggered -= HandlePerfectDodgeTriggered;
            }

            if (extraInvincibleRoutine != null)
            {
                StopCoroutine(extraInvincibleRoutine);
                extraInvincibleRoutine = null;
            }
        }

        private void ResolveReferences()
        {
            if (perfectDodgeController == null)
            {
                TryGetComponent(out perfectDodgeController);
            }

            if (perfectDodgeController == null)
            {
                perfectDodgeController = GetComponentInParent<PlayerPerfectDodgeController2D>();
            }

            if (slowMotionController == null)
            {
                slowMotionController = PerfectDodgeSlowMotionController.Instance;
            }

            if (slowMotionController == null)
            {
                slowMotionController = FindAnyObjectByType<PerfectDodgeSlowMotionController>();
            }

            if (dashController == null)
            {
                TryGetComponent(out dashController);
            }

            if (dashController == null)
            {
                dashController = GetComponentInParent<PlayerDashController2D>();
            }

            if (damageable == null)
            {
                TryGetComponent(out damageable);
            }

            if (damageable == null)
            {
                damageable = GetComponentInParent<Damageable>();
            }
        }

        private void HandlePerfectDodgeTriggered(PerfectDodgeEventData eventData)
        {
            if (slowMotionController != null)
            {
                slowMotionController.StartPerfectDodgeSlow();
            }

            if (extraInvincibleDuration > 0f && damageable != null)
            {
                if (extraInvincibleRoutine != null)
                {
                    StopCoroutine(extraInvincibleRoutine);
                }

                extraInvincibleRoutine = StartCoroutine(GrantExtraInvincibility());
            }

            if (logPerfectDodgeEffect)
            {
                Debug.Log("[PerfectDodge] Effect applied.", this);
            }
        }

        private IEnumerator GrantExtraInvincibility()
        {
            damageable.SetInvincible(true);
            float endTime = Time.unscaledTime + extraInvincibleDuration;

            while (Time.unscaledTime < endTime)
            {
                if (dashController == null || !dashController.IsDashing)
                {
                    damageable.SetInvincible(true);
                }

                yield return null;
            }

            if (dashController == null || !dashController.IsDashing)
            {
                damageable.SetInvincible(false);
            }

            extraInvincibleRoutine = null;
        }
    }
}
