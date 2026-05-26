using System;
using JingHongLu.Player;
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

        public event Action<DamageInfo> OnDamageTaken;
        public event Action<DamageInfo> OnDamageIgnoredByInvincibility;
        public event Action<DamageInfo> OnDamageTakenWithSuperArmor;

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
            if (IsDamageIgnoredByInvincibility())
            {
                OnDamageIgnoredByInvincibility?.Invoke(damageInfo);
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

            bool hasSuperArmor = HasSuperArmor();
            ShieldComponent shield = GetShieldComponent();

            if (shield != null && shield.HasShield)
            {
                shield.ApplyShieldDamage(damageInfo.ShieldDamage);

                if (shield.BlockHealthDamageWhileShielded)
                {
<<<<<<< HEAD
                    Debug.Log("[Shield] Health damage blocked while shielded.", shield);
=======
                    if (shield.CurrentShield > 0f)
                    {
                        shield.NotifyBlocked();
                        Debug.Log("[Shield] Health damage blocked while shielded.", shield);
                    }

                    ApplyKnockback(damageInfo, hasSuperArmor);
                    return;
>>>>>>> 2e37c94affc29f9f2e1e95f550cdf5c387f044a8
                }

                return;
            }

            ApplyHealthDamage(damageInfo, damageInfo.Damage, hasSuperArmor);
        }

        private void ApplyHealthDamage(
            DamageInfo damageInfo,
            float healthDamage,
            bool hasSuperArmor)
        {
            health.TakeDamage(healthDamage);

            OnDamageTaken?.Invoke(damageInfo);

            if (hasSuperArmor)
            {
                OnDamageTakenWithSuperArmor?.Invoke(damageInfo);
            }

            if (damageInfo.CanKnockUp && !hasSuperArmor)
            {
                ApplyKnockUp(damageInfo);
            }

            if (!health.IsDead)
            {
                ApplyHitStun(damageInfo);
                ApplyKnockback(damageInfo, hasSuperArmor);
            }

            Debug.Log(
                $"{name} took {healthDamage} damage from {damageInfo.SourceDisplayName}",
                this);
        }

        public void SetInvincible(bool value)
        {
            isInvincible = value;
        }

        private bool IsDamageIgnoredByInvincibility()
        {
            if (isInvincible)
            {
                return true;
            }

            PlayerInvincibilityController invincibilityController =
                GetComponentInParent<PlayerInvincibilityController>();

            return invincibilityController != null &&
                invincibilityController.IsInvincible;
        }

        private bool HasSuperArmor()
        {
            PlayerSuperArmorController superArmorController =
                GetComponentInParent<PlayerSuperArmorController>();

            return superArmorController != null &&
                superArmorController.HasSuperArmor;
        }

        private ShieldComponent GetShieldComponent()
        {
            ShieldComponent shield = GetComponentInParent<ShieldComponent>();

            if (shield != null)
            {
                return shield;
            }

            return GetComponent<ShieldComponent>();
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

        private void ApplyHitStun(DamageInfo damageInfo)
        {
            if (!damageInfo.CanApplyHitStun || damageInfo.HitStunDuration <= 0f)
            {
                return;
            }

            HitStunReceiver2D hitStunReceiver =
                GetComponentInParent<HitStunReceiver2D>();

            if (hitStunReceiver == null)
            {
                hitStunReceiver = GetComponent<HitStunReceiver2D>();
            }

            if (hitStunReceiver == null)
            {
                return;
            }

            hitStunReceiver.ApplyHitStun(damageInfo.HitStunDuration);
        }

        private void ApplyKnockback(DamageInfo damageInfo, bool hasSuperArmor)
        {
            if (hasSuperArmor ||
                !damageInfo.CanApplyKnockback ||
                damageInfo.KnockbackDistance <= 0f ||
                damageInfo.KnockbackDuration <= 0f)
            {
                return;
            }

            if (health != null && health.IsDead)
            {
                return;
            }

            EnemyKnockbackReceiver2D knockbackReceiver =
                GetComponentInParent<EnemyKnockbackReceiver2D>();

            if (knockbackReceiver == null)
            {
                knockbackReceiver = GetComponent<EnemyKnockbackReceiver2D>();
            }

            if (knockbackReceiver == null)
            {
                return;
            }

            knockbackReceiver.ApplyKnockback(
                damageInfo.KnockbackDirection,
                damageInfo.KnockbackDistance,
                damageInfo.KnockbackDuration);
        }
    }
}
