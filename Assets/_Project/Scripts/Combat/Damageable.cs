using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class Damageable : MonoBehaviour
    {
        [SerializeField] private TeamId team = TeamId.Enemy;
        [SerializeField] private Health health;
        [SerializeField] private bool isInvincible;

        public TeamId Team => team;
        public Health Health => health;
        public bool IsInvincible => isInvincible;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void ApplyDamage(DamageInfo damageInfo)
        {
            if (isInvincible)
            {
                return;
            }

            if (health == null)
            {
                ResolveReferences();
            }

            if (health == null)
            {
                Debug.LogWarning($"{name} received damage but has no Health component.", this);
                return;
            }

            health.TakeDamage(damageInfo.Damage);

            if (damageInfo.CanKnockUp)
            {
                ApplyKnockUp(damageInfo);
            }

            Debug.Log(
                $"{name} took {damageInfo.Damage} damage from {damageInfo.SourceDisplayName}",
                this);
        }

        public void SetInvincible(bool value)
        {
            isInvincible = value;
        }

        private void ResolveReferences()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (health == null)
            {
                health = GetComponentInParent<Health>();
            }
        }

        private void ApplyKnockUp(DamageInfo damageInfo)
        {
            AirborneTarget2D airborneTarget =
                GetComponentInParent<AirborneTarget2D>();

            if (airborneTarget != null)
            {
                airborneTarget.MarkAirborne(damageInfo.AirborneDuration);
            }

            Rigidbody2D body = GetComponentInParent<Rigidbody2D>();

            if (body == null)
            {
                return;
            }

            Vector2 velocity = damageInfo.KnockUpVelocity;
            float horizontalSign = ResolveKnockUpHorizontalSign(damageInfo);
            velocity.x = Mathf.Abs(velocity.x) * horizontalSign;
            velocity.y = Mathf.Abs(velocity.y);
            body.linearVelocity = velocity;
        }

        private float ResolveKnockUpHorizontalSign(DamageInfo damageInfo)
        {
            if (Mathf.Abs(damageInfo.KnockbackDirection.x) > 0.0001f)
            {
                return Mathf.Sign(damageInfo.KnockbackDirection.x);
            }

            if (damageInfo.Attacker != null)
            {
                float delta =
                    transform.position.x - damageInfo.Attacker.transform.position.x;

                if (Mathf.Abs(delta) > 0.0001f)
                {
                    return Mathf.Sign(delta);
                }
            }

            return 1f;
        }
    }
}
