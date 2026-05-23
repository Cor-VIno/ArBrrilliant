using System;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class EnemyKnockbackReceiver2D : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private bool logKnockback = true;

        private bool isKnockbacking;
        private float remainingTime;
        private float velocityX;

        public event Action OnKnockbackStarted;
        public event Action OnKnockbackEnded;

        public bool IsKnockbacking => isKnockbacking;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void FixedUpdate()
        {
            if (!isKnockbacking)
            {
                return;
            }

            remainingTime -= Time.fixedDeltaTime;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
            }

            if (remainingTime <= 0f)
            {
                EndKnockback();
            }
        }

        private void OnDisable()
        {
            ClearKnockback();
        }

        public void ApplyKnockback(Vector2 direction, float distance, float duration)
        {
            if (distance <= 0f || duration <= 0f)
            {
                return;
            }

            ResolveReferences();

            float sign = direction.x >= 0f ? 1f : -1f;
            velocityX = sign * distance / duration;
            remainingTime = duration;

            bool wasKnockbacking = isKnockbacking;
            isKnockbacking = true;

            if (!wasKnockbacking)
            {
                OnKnockbackStarted?.Invoke();
            }

            if (logKnockback)
            {
                Debug.Log(
                    $"[Knockback] Started. Distance={distance}, Duration={duration}",
                    this);
            }
        }

        private void EndKnockback()
        {
            if (!isKnockbacking)
            {
                return;
            }

            isKnockbacking = false;
            remainingTime = 0f;
            velocityX = 0f;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            OnKnockbackEnded?.Invoke();

            if (logKnockback)
            {
                Debug.Log("[Knockback] Ended.", this);
            }
        }

        private void ClearKnockback()
        {
            bool wasKnockbacking = isKnockbacking;
            isKnockbacking = false;
            remainingTime = 0f;
            velocityX = 0f;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            if (wasKnockbacking)
            {
                OnKnockbackEnded?.Invoke();
            }
        }

        private void ResolveReferences()
        {
            if (rb == null)
            {
                TryGetComponent(out rb);
            }

            if (rb == null)
            {
                rb = GetComponentInParent<Rigidbody2D>();
            }
        }
    }
}
