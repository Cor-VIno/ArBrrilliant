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
            SkillData sourceSkill)
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
    }
}
