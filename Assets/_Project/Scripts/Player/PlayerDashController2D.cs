using System.Collections;
using JingHongLu.Combat;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerDashController2D : MonoBehaviour
    {
        [SerializeField] private PlayerMotor2D motor;
        [SerializeField] private Damageable damageable;

        public bool IsDashing { get; private set; }

        private void Awake()
        {
            if (motor == null)
            {
                TryGetComponent(out motor);
            }

            if (damageable == null)
            {
                TryGetComponent(out damageable);
            }
        }

        public IEnumerator DashRoutine(DashData dashData, Vector2 direction)
        {
            if (dashData == null || IsDashing || motor == null)
            {
                yield break;
            }

            Vector2 dashDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.right;
            float dashDuration = Mathf.Max(0.01f, dashData.Duration);
            float dashSpeed = dashData.Distance / dashDuration;
            float elapsed = 0f;
            bool appliedInvincibility = dashData.InvincibleDuringDash && damageable != null;

            IsDashing = true;
            motor.BeginExternalMotion();

            if (appliedInvincibility)
            {
                damageable.SetInvincible(true);
            }

            while (elapsed < dashDuration)
            {
                motor.SetExternalVelocity(dashDirection * dashSpeed);
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            motor.SetExternalVelocity(
                dashDirection * dashSpeed * dashData.EndVelocityMultiplier);

            if (appliedInvincibility)
            {
                damageable.SetInvincible(false);
            }

            motor.EndExternalMotion();
            IsDashing = false;
        }
    }
}
