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

            string sourceSkillId = damageInfo.SourceSkill != null
                ? damageInfo.SourceSkill.SkillId
                : "unknown";
            Debug.Log($"{name} took {damageInfo.Damage} damage from {sourceSkillId}", this);
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
    }
}
