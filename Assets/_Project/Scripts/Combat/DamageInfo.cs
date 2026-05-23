using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            GameObject attacker,
            Damageable target,
            float damage,
            Vector2 hitPoint,
            Vector2 knockbackDirection,
            float knockbackForce,
            bool canCritical,
            bool isCritical,
            SkillData sourceSkill,
            string sourceDisplayName = null,
            bool canKnockUp = false,
            Vector2 knockUpVelocity = default,
            float airborneDuration = 0f,
            bool canApplyHitStun = false,
            float hitStunDuration = 0f,
            AttackInterruptType interruptType = AttackInterruptType.None,
            float shieldDamage = 0f,
            bool canApplyKnockback = false,
            float knockbackDistance = 0f,
            float knockbackDuration = 0f)
        {
            Attacker = attacker;
            Target = target;
            Damage = damage;
            HitPoint = hitPoint;
            KnockbackDirection = knockbackDirection;
            KnockbackForce = knockbackForce;
            CanCritical = canCritical;
            IsCritical = isCritical;
            SourceSkill = sourceSkill;
            SourceDisplayName = ResolveSourceDisplayName(sourceDisplayName, sourceSkill);
            CanKnockUp = canKnockUp;
            KnockUpVelocity = knockUpVelocity;
            AirborneDuration = airborneDuration;
            CanApplyHitStun = canApplyHitStun;
            HitStunDuration = hitStunDuration;
            InterruptType = interruptType;
            ShieldDamage = shieldDamage;
            CanApplyKnockback = canApplyKnockback;
            KnockbackDistance = knockbackDistance;
            KnockbackDuration = knockbackDuration;
        }

        public GameObject Attacker { get; }
        public Damageable Target { get; }
        public float Damage { get; }
        public Vector2 HitPoint { get; }
        public Vector2 KnockbackDirection { get; }
        public float KnockbackForce { get; }
        public bool CanCritical { get; }
        public bool IsCritical { get; }
        public SkillData SourceSkill { get; }
        public string SourceDisplayName { get; }
        public bool CanKnockUp { get; }
        public Vector2 KnockUpVelocity { get; }
        public float AirborneDuration { get; }
        public bool CanApplyHitStun { get; }
        public float HitStunDuration { get; }
        public AttackInterruptType InterruptType { get; }
        public float ShieldDamage { get; }
        public bool CanApplyKnockback { get; }
        public float KnockbackDistance { get; }
        public float KnockbackDuration { get; }

        private static string ResolveSourceDisplayName(
            string sourceDisplayName,
            SkillData sourceSkill)
        {
            if (!string.IsNullOrWhiteSpace(sourceDisplayName))
            {
                return sourceDisplayName;
            }

            if (sourceSkill != null)
            {
                return sourceSkill.DisplayName;
            }

            return "unknown";
        }
    }
}
